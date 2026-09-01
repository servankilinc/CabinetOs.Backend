using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.ComponentTemplate.Queries;

namespace CabinetOs.WebAPI.Utils;

/// <summary>
/// Sablon arka plan gorsellerinin diske yazilmasi.
///
/// Servis katmaninda DEGIL WebAPI katmaninda: <c>IFormFile</c> ve <c>wwwroot</c>
/// barindirma detaylaridir. <c>DiagramService</c>'in bir dosya sistemi bilmesi,
/// onu test edilemez ve bu barindirma bicimine bagimli hale getirirdi.
/// </summary>
public sealed class TemplateImageStore
{
    /// <summary>wwwroot altindaki klasor. URL yolu da bununla ayni.</summary>
    private const string RelativeFolder = "uploads/templates";

    /// <summary>
    /// 4 MB. Sablon gorseli bir bilesenin cizimidir, fotograf degil; bu sinirin
    /// ustu neredeyse kesinlikle yanlis dosya secildigi anlamina gelir.
    /// </summary>
    public const long MaxBytes = 4 * 1024 * 1024;

    /// <summary>
    /// Izin verilen uzantilar ve servis edilecek MIME tipleri.
    ///
    /// Beyaz liste, kara liste DEGIL: kara listede unutulan her uzanti aciktir.
    /// MIME tipi de BURADAN belirlenir, istemcinin <c>ContentType</c>'indan
    /// degil — istemci "image/png" deyip icine HTML koyabilir.
    /// </summary>
    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp",
        [".svg"] = "image/svg+xml"
    };

    public static string AllowedExtensionList => string.Join(", ", AllowedTypes.Keys);

    private readonly IWebHostEnvironment _environment;

    public TemplateImageStore(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Dosyayi yazar ve istemcinin kullanacagi GOreli URL'yi doner.
    ///
    /// Mutlak URL DONMEZ: sunucu adresi ortama gore degisir (localhost, IIS,
    /// ters vekil) ve veritabanina mutlak bir adres yazmak, tasindigi gun tum
    /// sablonlarin gorselini kirardi.
    /// </summary>
    public async Task<Result<TemplateImageDto>> SaveAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        // `file` null gelebilir: multipart govdede alan hic yoksa model binder
        // onu doldurmaz ve dogrulama yapilmadan `file.Length` okumak 500 uretirdi.
        if (file is null || file.Length == 0)
            return Result<TemplateImageDto>.Validation(Error("Dosya bos"));

        if (file.Length > MaxBytes)
            return Result<TemplateImageDto>.Validation(Error($"Dosya en fazla {MaxBytes / (1024 * 1024)} MB olabilir"));

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedTypes.ContainsKey(extension))
            return Result<TemplateImageDto>.Validation(Error($"Yalnizca su uzantilar kabul edilir: {AllowedExtensionList}"));

        // Dosya adi SUNUCUDA uretilir. Istemcinin adini kullanmak dizin
        // gezinmesine ("../../appsettings.json") ve ayni adli iki yuklemenin
        // birbirini ezmesine acik olurdu.
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        // WebRootPath, wwwroot yoksa null gelir — klasoru bu yuzden burada
        // olusturuyoruz, projede bos bir klasor tutmaya guvenmiyoruz (bos
        // klasorler kaynak kontrolunde kaybolur).
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, RelativeFolder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, fileName);
        await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return Result<TemplateImageDto>.Success(new TemplateImageDto { Url = $"/{RelativeFolder}/{fileName}" });
    }

    private static Dictionary<string, string[]> Error(string message)
        => new(StringComparer.Ordinal) { ["File"] = [message] };
}
