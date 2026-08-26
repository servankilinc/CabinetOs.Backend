using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Diagram.Commands;
using CabinetOs.Model.Dtos.Diagram.Queries;

namespace CabinetOs.Business.Abstract;

public interface IDiagramService
{
    Task<Result<DiagramDto>> GetAsync(Guid cabinetId, CancellationToken cancellationToken = default);

    /// <summary>Stencil kutuphanesi. Her kabinette ayni oldugu icin ayri uc + uzun staleTime.</summary>
    Task<Result<ICollection<ComponentTemplatePaletteDto>>> GetPaletteAsync(CancellationToken cancellationToken = default);

    /// <summary>Kabinin canvas tercihlerini yazar; kayit yoksa olusturur (upsert). </summary>
    Task<Result<DiagramCanvasSettingsDto>> UpsertCanvasSettingsAsync(Guid cabinetId, CanvasSettingsUpsertDto request, CancellationToken cancellationToken = default);

    Task<Result<DiagramSaveResponse>> SaveAsync(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken = default);

    /// <summary> Palet sablonunu ve pin semasini TEK transaction'da olusturur. </summary>
    Task<Result<CreatedDto>> CreateTemplateAsync(DiagramTemplateCreateRequest request, CancellationToken cancellationToken = default);
}
