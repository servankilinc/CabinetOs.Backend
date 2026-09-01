using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.ComponentTemplate.Commands;
using CabinetOs.Model.Entities;

namespace CabinetOs.Business.Concrete;

public partial class ComponentTemplateService
{
    /// <summary>
    /// Palet yazarligi: sablon + pinleri TEK transaction'da olusturur.
    ///
    /// Generic CRUD sablonunun <c>*AndSaveAsync</c> konvansiyonu burada BILEREK
    /// kirilir (ayni gerekce: <c>DiagramService.Save.cs</c>) — her pin icin ayri bir
    /// commit, yarim yazilmis bir sablon birakma riski demek olurdu.
    /// </summary>
    public async Task<Result<CreatedDto>> CreateAsync(
        ComponentTemplateCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<CreatedDto>.Validation(validationResult.Failures, description: "Validation failed for ComponentTemplateCreateRequest");

        // DeviceTypeId ONCE kontrol edilir. FK'ya birakilsaydi gecersiz bir tip
        // kisit ihlali uretir ve 500 donerdi; oysa bu, istemcinin duzeltebilecegi
        // siradan bir girdi hatasi. Ayni yaklasim DiagramService.SaveAsync'te de var:
        // referans dogrulamalari transaction ACILMADAN once yapilir.
        var deviceTypeExists = await _unitOfWork.DeviceTypes.IsExistAsync(
            where: t => t.Id == request.DeviceTypeId,
            cancellationToken: cancellationToken);

        if (!deviceTypeExists)
        {
            return Result<CreatedDto>.Validation(
                new Dictionary<string, string[]> { ["DeviceTypeId"] = ["Cihaz tipi bulunamadi"] },
                description: "Sablon cihaz tipi gecersiz");
        }

        var template = new ComponentTemplate
        {
            Name = request.Name,
            DeviceTypeId = request.DeviceTypeId,
            Width = request.Width,
            Height = request.Height,
            BackgroundColor = request.BackgroundColor,
            BackgroundImageUrl = request.BackgroundImageUrl,
            // Yeni sablon AKTIF dogar: pasif dogsaydi palette hic gorunmez ve
            // kullanici onu neden goremedigini anlamazdi (B5'te ayni kusur
            // Cabinet ve Device icin duzeltilmisti).
            IsActive = true
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            _unitOfWork.ComponentTemplates.Add(template);
            // Sablon ONCE yazilir: pinlerin FK'si icin gercek bir Id gerekiyor.
            // Iki SaveChanges tek transaction icinde — arada bir hata olursa
            // ikisi de geri alinir.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (request.Pins.Count > 0)
            {
                foreach (var draft in request.Pins)
                {
                    _unitOfWork.ComponentTemplatePins.Add(new ComponentTemplatePin
                    {
                        ComponentTemplateId = template.Id,
                        Name = draft.Name,
                        RelativeX = draft.RelativeX,
                        RelativeY = draft.RelativeY,
                        Side = draft.Side,
                        ChannelNumber = draft.ChannelNumber,
                        Function = draft.Function,
                        Direction = draft.Direction,
                        VoltageLevel = draft.VoltageLevel
                    });
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<CreatedDto>.Success(new CreatedDto(template.Id));
        }
        catch
        {
            // Yutulmaz, yeniden firlatilir: global ExceptionHandleMiddleware yigini
            // loglayip ProblemDetails uretiyor. Result.Failure'a cevirmek,
            // beklenmedik bir DB hatasinin izini silerdi.
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
