using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.ComponentTemplate.Commands;
using CabinetOs.Model.Dtos.ComponentTemplate.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface IComponentTemplateService
{
    /// <summary>Stencil kutuphanesi. Her kabinette ayni oldugu icin ayri uc + uzun staleTime.</summary>
    Task<Result<ICollection<ComponentTemplatePaletteDto>>> GetPaletteAsync(CancellationToken cancellationToken = default);

    /// <summary> Palet sablonunu ve pin semasini TEK transaction'da olusturur. </summary>
    Task<Result<CreatedDto>> CreateAsync(ComponentTemplateCreateRequest request, CancellationToken cancellationToken = default);

    Task<Result<ComponentTemplate>> GetAsync(Expression<Func<ComponentTemplate, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ComponentTemplate>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ComponentTemplateBaseDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ComponentTemplateDetailDto>> GetComponentTemplateDetailDtoAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ComponentTemplate>>> GetListAsync(Expression<Func<ComponentTemplate, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ComponentTemplate>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ComponentTemplateBaseDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ComponentTemplateDetailDto>>> GetComponentTemplateDetailDtoListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
}
