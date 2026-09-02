using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Diagram.Commands.Items;

/// <summary>
/// Yeni bir cihazin TEK bir telemetri kanali icin istemcinin urettigi kimlik.
///
/// <b>Neden pinin icine gomulu degil.</b> "Ayni cihazda ayni kanal numarasi TEK
/// bir <c>IoChannel</c>'dir" kurali (<c>IX_IoChannel_DeviceId_ChannelNumber</c>)
/// boyle YAPISAL olarak tutarsiz ifade edilemez hale gelir. Her pin kendi
/// <c>IoChannelId</c>'sini tasisaydi ayni kanali gosteren iki pinin ayni Id'yi
/// tasidigi her gonderide ayrica dogrulanmak zorunda kalirdi.
///
/// Sunucu kanal numarasi kumesinin, sablon pinlerinin null olmayan farkli kanal
/// numaralarina BIREBIR esit oldugunu dogrular.
/// </summary>
public class DeviceIoChannelDraft : IDto
{
    /// <summary> Olusacak <c>IoChannel</c> satirinin birincil anahtari. </summary>
    public Guid Id { get; set; }

    /// <summary> SCADA'nin kanali cozdugu numara; cihaz icinde benzersiz. </summary>
    public int ChannelNumber { get; set; }
}

public class DeviceIoChannelDraftValidator : AbstractValidator<DeviceIoChannelDraft>
{
    public DeviceIoChannelDraftValidator()
    {
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Kanal kimligi zorunlu");
    }
}
