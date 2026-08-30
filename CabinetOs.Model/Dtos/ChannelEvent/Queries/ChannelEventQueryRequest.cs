using CabinetOs.Core.Model;
using CabinetOs.Core.Utils.Pagination;
using FluentValidation;

namespace CabinetOs.Model.Dtos.ChannelEvent.Queries;

/// <summary>
/// Olay listesi sorgusu.
///
/// <b>Neden jenerik CRUD sablonu degil:</b> diger ~19 controller'in
/// <c>list/datatable</c> uclari <c>RepositoryBase</c>'in dinamik filtre motorunu
/// disariya aciyor. Olay tablosu buyuk ve append-only; serbest filtre burada
/// indekssiz taramalara acilan bir kapi olurdu. Bunun yerine uc, tam olarak
/// desteklenen iki indeksin (<c>CabinetId+OccurredAtUtc</c>,
/// <c>IoChannelId+OccurredAtUtc</c>) cevaplayabildigi sorulari kabul eder.
/// Bu, <c>DiagramController</c>'in "entity basina degil EKRAN basina" tasariminin
/// aynisidir.
/// </summary>
/// <remarks>
/// <c>PaginationRequest</c>'ten TUREMEZ: <c>IDto</c> bir arayuz degil, soyut bir
/// SINIF (<c>CabinetOs.Core.Model.IDto</c>) — ikisinden birden turemek mumkun
/// degil. Sayfalama alanlari burada tekrarlanir ve servis
/// <see cref="ToPaginationRequest"/> ile donusturur; boylece
/// <c>ToPaginateAsync</c> altyapisi aynen kullanilabilir.
/// </remarks>
public class ChannelEventQueryRequest : IDto
{
    /// <summary>Zorunlu — olaylar her zaman bir kabin baglaminda okunur.</summary>
    public Guid CabinetId { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    /// <summary>Tek bir kanala daraltmak icin; null ise kabinin tum kanallari.</summary>
    public Guid? IoChannelId { get; set; }

    /// <summary>Araligin basi (dahil), <c>OccurredAtUtc</c> uzerinden.</summary>
    public DateTime? FromUtc { get; set; }

    /// <summary>Araligin sonu (dahil).</summary>
    public DateTime? ToUtc { get; set; }

    /// <summary>Core'un sayfalama altyapisinin bekledigi sekle cevirir.</summary>
    public PaginationRequest ToPaginationRequest() => new() { Page = Page, PageSize = PageSize };
}

public class ChannelEventQueryRequestValidator : AbstractValidator<ChannelEventQueryRequest>
{
    /// <summary>
    /// Sayfa boyutu tavani. Sinirsiz birakmak, tek istekle tum olay gecmisini
    /// cekmeye izin vermek olurdu.
    /// </summary>
    private const int MaxPageSize = 200;

    public ChannelEventQueryRequestValidator()
    {
        RuleFor(v => v.CabinetId).NotEqual(Guid.Empty).WithMessage("Kabin bilgisi zorunlu");
        RuleFor(v => v.Page).GreaterThan(0).WithMessage("Sayfa numarası sıfırdan büyük olmalı");
        RuleFor(v => v.PageSize).InclusiveBetween(1, MaxPageSize)
            .WithMessage($"Sayfa boyutu 1 ile {MaxPageSize} arasında olmalı");
        RuleFor(v => v.ToUtc).GreaterThanOrEqualTo(v => v.FromUtc!.Value)
            .When(v => v.FromUtc.HasValue && v.ToUtc.HasValue)
            .WithMessage("Bitiş tarihi başlangıçtan önce olamaz");
    }
}
