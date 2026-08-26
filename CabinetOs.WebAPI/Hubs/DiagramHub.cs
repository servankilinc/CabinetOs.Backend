using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CabinetOs.WebAPI.Hubs;

/// <summary>
/// Diyagramin canli kanali: <c>/hubs/diagram</c>.
///
/// <b>Kabin bazli gruplar.</b> Herkese yayin yapmak, bir kabinin telemetrisini o
/// kabini hic acmamis her istemciye gondermek olurdu — hem gereksiz trafik hem
/// sizinti. Istemci acikca <see cref="Subscribe"/> cagirir.
///
/// <b>[Authorize] BaseController'dan gelmez</b> — bu bir hub, controller degil;
/// oznitelik burada acikca yaziliyor. WebSocket el sikismasi <c>Authorization</c>
/// header'i tasiyamadigi icin token <c>access_token</c> query string'inden okunur;
/// bu yapilmazsa hub SESSIZCE 401 verir (bkz. <c>Program.cs</c>, OnMessageReceived).
///
/// Sozlesme: <c>docs/api-contract/09-realtime.md</c>
/// </summary>
[Authorize]
public class DiagramHub : Hub<IDiagramClient>
{
    /// <summary>Grup adi tek yerde uretilir — yayinci ve abone ayni dizeyi kullanmak ZORUNDA.</summary>
    public static string GroupName(Guid cabinetId) => $"cabinet:{cabinetId}";

    public Task Subscribe(Guid cabinetId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(cabinetId), Context.ConnectionAborted);

    public Task Unsubscribe(Guid cabinetId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(cabinetId), Context.ConnectionAborted);

    // Baglanti kopunca gruplardan cikis icin ek kod YOK: SignalR, kopan bir
    // baglantiyi tum gruplarindan kendisi dusurur. Elle yapmaya calismak, yeniden
    // baglanan istemcinin yeni ConnectionId'siyle yaris uretirdi.
}
