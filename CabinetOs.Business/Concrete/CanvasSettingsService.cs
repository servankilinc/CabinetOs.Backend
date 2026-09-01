using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.CanvasSettings.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class CanvasSettingsService : ICanvasSettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public CanvasSettingsService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CanvasSettings>> GetAsync(Expression<Func<CanvasSettings, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.CanvasSettings.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<CanvasSettings>.NotFound();
        return Result<CanvasSettings>.Success(result);
    }

    public async Task<Result<CanvasSettings>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.CanvasSettings.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<CanvasSettings>.NotFound();
        return Result<CanvasSettings>.Success(result);
    }

    public async Task<Result<CanvasSettingsDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.CanvasSettings.GetAsync<CanvasSettingsDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<CanvasSettingsDto>.NotFound();
        return Result<CanvasSettingsDto>.Success(result);
    }

    public async Task<Result<ICollection<CanvasSettings>>> GetListAsync(Expression<Func<CanvasSettings, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.CanvasSettings.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<CanvasSettings>>.NotFound();
        return Result<ICollection<CanvasSettings>>.Success(result);
    }

    public async Task<Result<ICollection<CanvasSettings>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.CanvasSettings.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<CanvasSettings>>.NotFound();
        return Result<ICollection<CanvasSettings>>.Success(result);
    }

    public async Task<Result<ICollection<CanvasSettingsDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.CanvasSettings.GetAllAsync<CanvasSettingsDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<CanvasSettingsDto>>.NotFound();
        return Result<ICollection<CanvasSettingsDto>>.Success(result);
    }
}
