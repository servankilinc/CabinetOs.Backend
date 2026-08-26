using CabinetOs.Model.Dtos.Realtime.Queries;

namespace CabinetOs.WebAPI.Hubs;

/// <summary>
/// Hub'in ISTEMCI yuzu — tipli hub istemcisi.
///
/// <c>Clients.Group(...).SendAsync("ChannelValuesChanged", ...)</c> yerine bunu
/// kullanmanin sebebi: string metot adi yazim hatasini derleyici yakalamaz,
/// yayin sessizce hicbir yere gitmez ve hata ancak arayuzde "veri gelmiyor"
/// olarak fark edilir.
///
/// Metot adlari SOZLESMEDIR: frontend <c>connection.on('&lt;ad&gt;', …)</c> ile
/// tam olarak bunlari dinler. Bkz. <c>docs/api-contract/09-realtime.md</c>.
/// </summary>
public interface IDiagramClient
{
    Task ChannelValuesChanged(Guid cabinetId, IReadOnlyList<ChannelValueChange> changes);

    Task DeviceStatusChanged(Guid cabinetId, IReadOnlyList<DeviceStatusChange> changes);

    Task CabinetStatusChanged(CabinetStatusChange change);

    Task CommandCompleted(Guid cabinetId, CommandCompleted change);
}
