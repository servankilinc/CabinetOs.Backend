using CabinetOs.Business.Utils.DiagramNotifier;
using CabinetOs.Model.Dtos.Realtime.Queries;
using Microsoft.AspNetCore.SignalR;

namespace CabinetOs.WebAPI.Hubs;

/// <summary>
/// <see cref="IDiagramNotifier"/> portunun SignalR implementasyonu.
///
/// Business katmani bu sinifi gormez; yalnizca portu bilir. Boylece ingest'in is
/// kurallari bir tasima teknolojisine baglanmaz ve testte gercek bir hub ayaga
/// kaldirilmasi gerekmez.
/// </summary>
public class DiagramHubNotifier : IDiagramNotifier
{
    private readonly IHubContext<DiagramHub, IDiagramClient> _hub;
    private readonly ILogger<DiagramHubNotifier> _logger;

    public DiagramHubNotifier(IHubContext<DiagramHub, IDiagramClient> hub, ILogger<DiagramHubNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public Task ChannelValuesChangedAsync(Guid cabinetId, IReadOnlyList<ChannelValueChange> changes, CancellationToken cancellationToken = default) =>
        Publish(cabinetId, client => client.ChannelValuesChanged(cabinetId, changes), nameof(ChannelValuesChangedAsync));

    public Task DeviceStatusesChangedAsync(Guid cabinetId, IReadOnlyList<DeviceStatusChange> changes, CancellationToken cancellationToken = default) =>
        Publish(cabinetId, client => client.DeviceStatusChanged(cabinetId, changes), nameof(DeviceStatusesChangedAsync));

    public Task CabinetStatusChangedAsync(CabinetStatusChange change, CancellationToken cancellationToken = default) =>
        Publish(change.CabinetId, client => client.CabinetStatusChanged(change), nameof(CabinetStatusChangedAsync));

    public Task CommandCompletedAsync(Guid cabinetId, CommandCompleted change, CancellationToken cancellationToken = default) =>
        Publish(cabinetId, client => client.CommandCompleted(cabinetId, change), nameof(CommandCompletedAsync));

    /// <summary>
    /// <b>Yayin hatasi ingest'i DUSURMEZ.</b> Telemetri veritabanina yazildiktan
    /// sonra hub'a ulasamamak, SCADA'ya 500 dondurmek icin bir sebep degil: SCADA o
    /// gonderiyi tekrar dener ve zaten yazilmis veriyi yeniden yazmaya calisirdi.
    /// Kaybedilen tek sey bir guncelleme karesidir; bir sonraki ingest onu kapatir.
    ///
    /// Yine de <b>loglanir</b> — sessiz yutma, "canli veri gelmiyor" sikayetinin
    /// sebebini bulunamaz kilardi.
    /// </summary>
    private async Task Publish(Guid cabinetId, Func<IDiagramClient, Task> send, string operation)
    {
        try
        {
            await send(_hub.Clients.Group(DiagramHub.GroupName(cabinetId)));
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Canli yayin basarisiz: {Operation}, kabin {CabinetId}", operation, cabinetId);
        }
    }
}
