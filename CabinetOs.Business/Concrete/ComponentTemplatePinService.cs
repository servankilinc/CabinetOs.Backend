using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.ComponentTemplatePin.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class ComponentTemplatePinService : IComponentTemplatePinService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public ComponentTemplatePinService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ComponentTemplatePin>> GetAsync(Expression<Func<ComponentTemplatePin, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplatePin>.NotFound();
        return Result<ComponentTemplatePin>.Success(result);
    }

    public async Task<Result<ComponentTemplatePin>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplatePin>.NotFound();
        return Result<ComponentTemplatePin>.Success(result);
    }

    public async Task<Result<ComponentTemplatePinDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAsync<ComponentTemplatePinDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplatePinDto>.NotFound();
        return Result<ComponentTemplatePinDto>.Success(result);
    }

    public async Task<Result<ICollection<ComponentTemplatePin>>> GetListAsync(Expression<Func<ComponentTemplatePin, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ComponentTemplatePin>>.NotFound();
        return Result<ICollection<ComponentTemplatePin>>.Success(result);
    }

    public async Task<Result<ICollection<ComponentTemplatePin>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ComponentTemplatePin>>.NotFound();
        return Result<ICollection<ComponentTemplatePin>>.Success(result);
    }

    public async Task<Result<ICollection<ComponentTemplatePinDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAllAsync<ComponentTemplatePinDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ComponentTemplatePinDto>>.NotFound();
        return Result<ICollection<ComponentTemplatePinDto>>.Success(result);
    }
}
