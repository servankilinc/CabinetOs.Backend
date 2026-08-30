using CabinetOs.Business.Abstract;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.ChannelEvent.Queries;

namespace CabinetOs.Business.Concrete;

/// <summary>
/// Kanal olaylarinin okuma yolu.
///
/// Sozlesme: <c>docs/api-contract/12-channel-events.md</c>
/// </summary>
public class ChannelEventService : IChannelEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;

    public ChannelEventService(IUnitOfWork unitOfWork, IValidationService validationService)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
    }

    public async Task<Result<PaginationResponse<ChannelEventDto>>> GetPagedAsync(
        ChannelEventQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<PaginationResponse<ChannelEventDto>>.Validation(validationResult.Failures, description: "Validation failed for ChannelEventQueryRequest");

        // Kabin kontrolu ONCE: aksi halde var olmayan bir kabin icin bos liste
        // donerdi ve "kabin yok" ile "kabinde olay yok" ayirt edilemezdi.
        // Pasif kabin DAHIL EDILIR — pasife alinmis bir kabinin gecmisi de okunabilmeli.
        var cabinetExists = await _unitOfWork.Cabinets.IsExistAsync(
            where: c => c.Id == request.CabinetId,
            cancellationToken: cancellationToken);

        if (!cabinetExists)
            return Result<PaginationResponse<ChannelEventDto>>.NotFound(description: "Kabin bulunamadi");

        var page = await _unitOfWork.ChannelEvents.GetPagedAsync(
            request.CabinetId,
            request.IoChannelId,
            request.FromUtc,
            request.ToUtc,
            request.ToPaginationRequest(),
            cancellationToken);

        return Result<PaginationResponse<ChannelEventDto>>.Success(page);
    }
}
