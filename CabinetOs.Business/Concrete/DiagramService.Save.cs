using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Diagram.Commands;
using CabinetOs.Model.Dtos.Diagram.Queries;

namespace CabinetOs.Business.Concrete;

public partial class DiagramService
{
    /// <summary>
    /// Diyagram editorunun toplu kaydetme yolu — kod tabanindaki ILK gercek
    /// cok-varlikli transaction.
    ///
    /// Generic CRUD sablonunun <c>*AndSaveAsync</c> konvansiyonu burada BILEREK
    /// kirilir: o metotlarin her biri kendi <c>SaveChanges</c>'ini cagirir ve tek bir
    /// kaydetme icin sekiz ayri commit uretirdi.
    ///
    /// Bu metodun kullandigi ic adimlar <c>DiagramService.SaveInternals.cs</c>'te.
    /// </summary>
    public async Task<Result<DiagramSaveResponse>> SaveAsync(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<DiagramSaveResponse>.Validation(validationResult.Failures, description: "Validation failed for DiagramSaveRequest");

        var cabinetExists = await _unitOfWork.Cabinets.IsExistAsync(
            where: c => c.Id == cabinetId && c.IsActive,
            cancellationToken: cancellationToken);

        if (!cabinetExists)
            return Result<DiagramSaveResponse>.NotFound(description: "Kabin bulunamadi veya pasif durumda");

        // Bos gonderi: transaction bile acilmaz. Kaydet dugmesi bos bir gunlukle
        // tetiklendiginde bunu 400 ile cezalandirmak yalnizca gurultu uretir.
        if (request.IsEmpty)
            return Result<DiagramSaveResponse>.Success(new DiagramSaveResponse { SavedAtUtc = DateTime.UtcNow });

        var context = await LoadSaveContextAsync(cabinetId, request, cancellationToken);

        // Referans dogrulamalari YAZMADAN ONCE yapilir: transaction'i acip sonra
        // geri almak yerine hic acmamak, kilit suresini de log gurultusunu de azaltir.
        var errors = ValidateReferences(request, context);
        if (errors.Count > 0)
            return Result<DiagramSaveResponse>.Validation(errors, description: "Diyagram kaydetme referanslari gecersiz");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ---- FAZ 1: SILMELER ----
            // Silmeler ve yazmalar AYRI SaveChanges'lerde akitilir. Sebep filtreli
            // unique index: IX_Connection_SourcePinId_TargetPinId "WHERE IsDeleted = 0"
            // ile calisir. Kullanici bir kabloyu silip AYNI iki pin arasina yenisini
            // cizdiginde (cizdi-vazgecti-yeniden cizdi, editorde siradan bir dizi),
            // silme bir UPDATE, olusturma bir INSERT'tur; tek batch'te EF'in sirasi
            // garanti degildir ve INSERT once giderse index ihlali 500 doner.
            var skippedDeleteCount = ApplyDeletions(request, context);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ---- FAZ 2: OLUSTURMALAR + GUNCELLEMELER ----
            var instantiatedPinCount = ApplyUpserts(cabinetId, request, context);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<DiagramSaveResponse>.Success(new DiagramSaveResponse
            {
                InstantiatedPinCount = instantiatedPinCount,
                SkippedDeleteCount = skippedDeleteCount,
                SavedAtUtc = DateTime.UtcNow
            });
        }
        catch
        {
            // Yutulmaz, yeniden firlatilir: global ExceptionHandleMiddleware yigini
            // loglayip ProblemDetails uretiyor. Burada Result.Failure'a cevirmek,
            // beklenmedik bir DB hatasinin izini silerdi.
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
