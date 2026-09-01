using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Pin.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class PinService : IPinService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public PinService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<Pin>> GetAsync(Expression<Func<Pin, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<Pin>.NotFound();
        return Result<Pin>.Success(result);
    }

    public async Task<Result<Pin>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<Pin>.NotFound();
        return Result<Pin>.Success(result);
    }

    public async Task<Result<PinDetailDto>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAsync<PinDetailDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<PinDetailDto>.NotFound();
        return Result<PinDetailDto>.Success(result);
    }

    public async Task<Result<PinDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAsync<PinDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<PinDto>.NotFound();
        return Result<PinDto>.Success(result);
    }

    public async Task<Result<ICollection<Pin>>> GetListAsync(Expression<Func<Pin, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<Pin>>.NotFound();
        return Result<ICollection<Pin>>.Success(result);
    }

    public async Task<Result<ICollection<Pin>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<Pin>>.NotFound();
        return Result<ICollection<Pin>>.Success(result);
    }

    public async Task<Result<ICollection<PinDetailDto>>> GetDetailListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAllAsync<PinDetailDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<PinDetailDto>>.NotFound();
        return Result<ICollection<PinDetailDto>>.Success(result);
    }

    public async Task<Result<ICollection<PinDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAllAsync<PinDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<PinDto>>.NotFound();
        return Result<ICollection<PinDto>>.Success(result);
    }
}
