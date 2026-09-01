using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.IoChannel.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class IoChannelService : IIoChannelService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public IoChannelService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IoChannel>> GetAsync(Expression<Func<IoChannel, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.IoChannels.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<IoChannel>.NotFound();
        return Result<IoChannel>.Success(result);
    }

    public async Task<Result<IoChannel>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.IoChannels.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<IoChannel>.NotFound();
        return Result<IoChannel>.Success(result);
    }

    public async Task<Result<IoChannelDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.IoChannels.GetAsync<IoChannelDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<IoChannelDto>.NotFound();
        return Result<IoChannelDto>.Success(result);
    }

    public async Task<Result<ICollection<IoChannel>>> GetListAsync(Expression<Func<IoChannel, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.IoChannels.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<IoChannel>>.NotFound();
        return Result<ICollection<IoChannel>>.Success(result);
    }

    public async Task<Result<ICollection<IoChannel>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.IoChannels.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<IoChannel>>.NotFound();
        return Result<ICollection<IoChannel>>.Success(result);
    }

    public async Task<Result<ICollection<IoChannelDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.IoChannels.GetAllAsync<IoChannelDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<IoChannelDto>>.NotFound();
        return Result<ICollection<IoChannelDto>>.Success(result);
    }
}
