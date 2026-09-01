using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.ComponentTemplate.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class ComponentTemplateService : IComponentTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public ComponentTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ComponentTemplate>> GetAsync(Expression<Func<ComponentTemplate, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplates.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplate>.NotFound();
        return Result<ComponentTemplate>.Success(result);
    }

    public async Task<Result<ComponentTemplate>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplates.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplate>.NotFound();
        return Result<ComponentTemplate>.Success(result);
    }

    public async Task<Result<ComponentTemplateBaseDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplates.GetAsync<ComponentTemplateBaseDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplateBaseDto>.NotFound();
        return Result<ComponentTemplateBaseDto>.Success(result);
    }

    public async Task<Result<ComponentTemplateDetailDto>> GetComponentTemplateDetailDtoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplates.GetAsync<ComponentTemplateDetailDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplateDetailDto>.NotFound();
        return Result<ComponentTemplateDetailDto>.Success(result);
    }

    public async Task<Result<ICollection<ComponentTemplate>>> GetListAsync(Expression<Func<ComponentTemplate, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplates.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ComponentTemplate>>.NotFound();
        return Result<ICollection<ComponentTemplate>>.Success(result);
    }

    public async Task<Result<ICollection<ComponentTemplate>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplates.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ComponentTemplate>>.NotFound();
        return Result<ICollection<ComponentTemplate>>.Success(result);
    }

    public async Task<Result<ICollection<ComponentTemplateBaseDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplates.GetAllAsync<ComponentTemplateBaseDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ComponentTemplateBaseDto>>.NotFound();
        return Result<ICollection<ComponentTemplateBaseDto>>.Success(result);
    }

    public async Task<Result<ICollection<ComponentTemplateDetailDto>>> GetComponentTemplateDetailDtoListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplates.GetAllAsync<ComponentTemplateDetailDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ComponentTemplateDetailDto>>.NotFound();
        return Result<ICollection<ComponentTemplateDetailDto>>.Success(result);
    }
}
