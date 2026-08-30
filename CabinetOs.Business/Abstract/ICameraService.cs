using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Camera.Commands;
using CabinetOs.Model.Dtos.Camera.Queries;
using CabinetOs.Model.Dtos.Common;

namespace CabinetOs.Business.Abstract;

/// <summary>
/// Kamera yonetimi.
///
/// <b>Jenerik CRUD sablonundan bilerek ayrilir</b> (datatable, dinamik filtre,
/// selectlist yok): kamera EKRAN basina tasarlanmis bir kaynaktir ve tek bir
/// listeleme sorusu vardir — "bu kabinde hangi kameralar var".
/// <c>DiagramController</c> ile ayni tercih.
///
/// K1'in tenant hazirlik kurali gecerli: <c>companyId</c> parametresi YOK,
/// <c>IgnoreQueryFilters</c> YOK.
/// </summary>
public interface ICameraService
{
    /// <summary>
    /// Kabindeki kameralar. <paramref name="includePassive"/> false ise yalnizca
    /// aktif olanlar. Pasifleri gorebilmek sart — pasife alinan bir kaydi geri
    /// getirmenin baska yolu yok.
    /// </summary>
    Task<Result<ICollection<CameraDto>>> GetListAsync(Guid cabinetId, bool includePassive = false, CancellationToken cancellationToken = default);

    Task<Result<CameraDto>> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Yeni kamera. Olusan Id doner — arayuz kaydi hemen secebilsin diye.</summary>
    Task<Result<CreatedDto>> CreateAsync(CameraCreateDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kamerayi gunceller. Pasife alma da buradan yapilir
    /// (<c>IsActive = false</c>); ayri bir DELETE ucu yoktur.
    /// </summary>
    Task<Result> UpdateAsync(CameraUpdateDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir yoklama (ping / TCP connect) sonucunu yazar.
    ///
    /// <b>Bu turda cagiran bir arka plan servisi YOKTUR</b> — kullanici kendi
    /// yoklayicisini yazacak. Uc ve servis metodu, o yoklayicinin in-process mi
    /// yoksa harici mi olacagindan bagimsiz hedefidir.
    ///
    /// Durum DEGISMEDIYSE hicbir yazma yapilmaz: 5 dakikada bir yoklanan, surekli
    /// ayakta bir kamera sifir UPDATE uretir.
    /// </summary>
    Task<Result> RecordProbeResultAsync(Guid cameraId, CameraProbeResultDto request, CancellationToken cancellationToken = default);
}
