using CabinetOs.Core.Model;
using CabinetOs.Core.Utils.CriticalData;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Camera.Commands;

/// <summary>
/// Yeni kamera. Sozlesme: <c>docs/api-contract/11-camera.md</c>
///
/// Hicbir varsayilan (554, 80, 101/102) sunucu kodunda SABIT DEGILDIR; hepsi
/// burada varsayilan deger olarak durur ve istemci degistirebilir. Sahada NVR
/// arkasindaki bir kamerada kanal numaralari farklidir.
/// </summary>
public class CameraCreateDto : IDto, ICameraWritableFields
{
    public Guid CabinetId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Manufacturer { get; set; } = "Hikvision";
    public string? Model { get; set; }

    public string IpAddress { get; set; } = null!;
    public int RtspPort { get; set; } = 554;
    public int HttpPort { get; set; } = 80;
    public int? HttpsPort { get; set; }

    public string? Username { get; set; }

    /// <summary>
    /// Kamera parolasi — yalnizca YAZMA yonunde tasinir, hicbir okuma DTO'sunda
    /// geri donmez. <c>[CriticalData]</c> loglama katmaninda maskelenmesini saglar.
    /// </summary>
    [CriticalData]
    public string? Password { get; set; }

    public int MainStreamChannel { get; set; } = 101;
    public int SubStreamChannel { get; set; } = 102;
    public bool MainStreamEnabled { get; set; } = true;
    public bool SubStreamEnabled { get; set; } = true;
    public int SnapshotChannel { get; set; } = 101;

    /// <summary>Yoklama sondasinin portu; bos birakilirsa RTSP portu kullanilir.</summary>
    public int? MonitoringPort { get; set; }
    public int PingIntervalSec { get; set; } = 300;
    public bool IsMonitoringEnabled { get; set; } = true;
}

public class CameraCreateDtoValidator : AbstractValidator<CameraCreateDto>
{
    public CameraCreateDtoValidator()
    {
        RuleFor(v => v.CabinetId).NotEqual(Guid.Empty).WithMessage("Kabin bilgisi zorunlu");
        RuleFor(v => v.Name).NotEmpty().WithMessage("İsim bilgisi girilmeli");
        RuleFor(v => v.Name).MaximumLength(150).WithMessage("İsim en fazla 150 karakter olabilir");
        CameraRules.Apply(this);
    }
}

/// <summary>
/// Create ve Update'in ORTAK kurallari.
///
/// Tek yerde durmalari sart: ikisi ayrisirsa create'te reddedilen bir deger
/// update ile iceri girebilir (ya da tersi) ve fark ancak sahada goze carpardi.
/// </summary>
internal static class CameraRules
{
    public static void Apply<T>(AbstractValidator<T> v) where T : ICameraWritableFields
    {
        v.RuleFor(x => x.IpAddress).NotEmpty().WithMessage("IP adresi girilmeli");
        v.RuleFor(x => x.IpAddress).MaximumLength(64).WithMessage("IP adresi en fazla 64 karakter olabilir");

        // Port araligi 1..65535 — 0 gecerli bir TCP portu degil.
        v.RuleFor(x => x.RtspPort).InclusiveBetween(1, 65535).WithMessage("RTSP portu 1-65535 arasında olmalı");
        v.RuleFor(x => x.HttpPort).InclusiveBetween(1, 65535).WithMessage("HTTP portu 1-65535 arasında olmalı");
        v.RuleFor(x => x.HttpsPort!.Value).InclusiveBetween(1, 65535).When(x => x.HttpsPort.HasValue)
            .WithMessage("HTTPS portu 1-65535 arasında olmalı");
        v.RuleFor(x => x.MonitoringPort!.Value).InclusiveBetween(1, 65535).When(x => x.MonitoringPort.HasValue)
            .WithMessage("İzleme portu 1-65535 arasında olmalı");

        v.RuleFor(x => x.MainStreamChannel).GreaterThan(0).WithMessage("Ana akım kanalı sıfırdan büyük olmalı");
        v.RuleFor(x => x.SubStreamChannel).GreaterThan(0).WithMessage("Tali akım kanalı sıfırdan büyük olmalı");
        v.RuleFor(x => x.SnapshotChannel).GreaterThan(0).WithMessage("Anlık görüntü kanalı sıfırdan büyük olmalı");

        // En az bir akim acik olmali; ikisi de kapaliysa kamera hic izlenemez ve
        // arayuz sebebini gosteremez.
        v.RuleFor(x => x.MainStreamEnabled).Must((x, _) => x.MainStreamEnabled || x.SubStreamEnabled)
            .WithMessage("Ana akım ve tali akım aynı anda kapatılamaz");

        // 10 sn'nin altinda bir yoklama araligi, kameraya faydasiz yuk bindirir;
        // 24 saatin ustunde ise "izleniyor" demek anlamsizlasir.
        v.RuleFor(x => x.PingIntervalSec).InclusiveBetween(10, 86400)
            .WithMessage("Yoklama aralığı 10 saniye ile 24 saat arasında olmalı");

        v.RuleFor(x => x.Username).MaximumLength(128).WithMessage("Kullanıcı adı en fazla 128 karakter olabilir");
        v.RuleFor(x => x.Manufacturer).MaximumLength(64).WithMessage("Üretici en fazla 64 karakter olabilir");
        v.RuleFor(x => x.Model).MaximumLength(64).WithMessage("Model en fazla 64 karakter olabilir");
        v.RuleFor(x => x.Description).MaximumLength(512).WithMessage("Açıklama en fazla 512 karakter olabilir");
    }
}

/// <summary>
/// Create ve Update DTO'larinin ortak yazilabilir alanlari — yalnizca
/// <see cref="CameraRules"/>'in ikisine birden uygulanabilmesi icin var.
/// </summary>
public interface ICameraWritableFields
{
    string IpAddress { get; }
    int RtspPort { get; }
    int HttpPort { get; }
    int? HttpsPort { get; }
    int? MonitoringPort { get; }
    int MainStreamChannel { get; }
    int SubStreamChannel { get; }
    int SnapshotChannel { get; }
    bool MainStreamEnabled { get; }
    bool SubStreamEnabled { get; }
    int PingIntervalSec { get; }
    string? Username { get; }
    string? Manufacturer { get; }
    string? Model { get; }
    string? Description { get; }
}
