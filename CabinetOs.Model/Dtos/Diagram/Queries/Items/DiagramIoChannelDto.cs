using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.Diagram.Queries.Items;

/// <summary>
/// Cihazin telemetri kanalinin STATIK tanimi.
///
/// <c>CurrentValue</c> ve <c>ValueUpdatedAt</c> BILEREK YOK. Canli deger ayri bir
/// kanaldan (SignalR) akar ve istemcide TanStack Query cache'ine degil harici bir store'a yazilir.
/// </summary>
public class DiagramIoChannelDto : IDto
{
    public Guid Id { get; set; }
    public int ChannelNumber { get; set; }
    public PinDirection Direction { get; set; }
    public bool IsEnabled { get; set; }
    public string Name { get; set; } = null!;
}
