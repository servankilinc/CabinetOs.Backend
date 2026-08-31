using CabinetOs.Core.Model;
using CabinetOs.Core.Utils.CriticalData;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Camera.Commands;

/// <summary>
/// Kamera guncelleme. Sozlesme: <c>docs/api-contract/11-camera.md</c>
///
/// <b><c>CabinetId</c> YOKTUR ve degistirilemez.</b> Kamera fiziksel olarak bir
/// kabinin icindedir; kabin degistirmek "ayni kamera" degil "baska bir kurulum"
/// demektir ve gecmis cekimlerini (<c>CameraCapture</c>) yanlis kabine baglardi.
/// Tasima gerekirse eski kayit pasife alinip yenisi acilir.
/// </summary>
public class CameraUpdateDto : IDto, ICameraWritableFields
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }

    public string IpAddress { get; set; } = null!;
    public int RtspPort { get; set; }
    public int HttpPort { get; set; }
    public int? HttpsPort { get; set; }

    public string? Username { get; set; }

    /// <summary>
    /// Yeni parola.
    ///
    /// <b><c>null</c> = "dokunma"</b>, mevcut parola korunur. Bos string ise
    /// parola SILINIR. Bu ayrim sart: okuma DTO'su parolayi hic dondurmedigi
    /// icin arayuz formu doldururken alani bos birakir; <c>null</c>'i "sil"
    /// saymak, her duzenlemede parolayi sessizce ucururdu.
    /// </summary>
    [CriticalData]
    public string? Password { get; set; }

    public int MainStreamChannel { get; set; }
    public int SubStreamChannel { get; set; }
    public bool MainStreamEnabled { get; set; }
    public bool SubStreamEnabled { get; set; }
    public int SnapshotChannel { get; set; }

    public int? MonitoringPort { get; set; }
    public int PingIntervalSec { get; set; }
    public bool IsMonitoringEnabled { get; set; }

    /// <summary>
    /// Pasife almak icin <c>false</c>. Ayri bir DELETE ucu YOKTUR — kod tabaninin
    /// B5'te aldigi kararla ayni: <c>Camera</c> <c>IActivatableEntity</c>'dir,
    /// fiziksel silme interceptor'da exception atar.
    /// </summary>
    public bool IsActive { get; set; }
}

public class CameraUpdateDtoValidator : AbstractValidator<CameraUpdateDto>
{
    public CameraUpdateDtoValidator()
    {
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Geçersiz kamera bilgisi");
        RuleFor(v => v.Name).NotEmpty().WithMessage("İsim bilgisi girilmeli");
        RuleFor(v => v.Name).MaximumLength(150).WithMessage("İsim en fazla 150 karakter olabilir");
        CameraRules.Apply(this);
    }
}
