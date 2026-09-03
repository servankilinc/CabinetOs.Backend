using CabinetOs.Business.Utils.SnapshotGateway;
using CabinetOs.Core.Utils.ResultPattern;
using Microsoft.Extensions.Caching.Distributed;

namespace CabinetOs.Business.Concrete;

public partial class CameraService
{
    /// <summary>
    /// Anlik goruntu — canli onizleme icin.
    /// </summary>
    /// <param name="fresh">
    /// <c>true</c> ise onbellek atlanir. Operator "goruntuyu tazele" dedigi
    /// zaman 3 saniyelik de olsa eski bir kare gormemeli.
    /// </param>
    public async Task<Result<SnapshotPayload>> GetSnapshotAsync(Guid cameraId, bool fresh = false, CancellationToken cancellationToken = default)
    {
        string cacheKey = SnapshotCacheKeyPrefix + cameraId;
        string typeCacheKey = SnapshotTypeCacheKeyPrefix + cameraId;

        if (!fresh)
        {
            var cached = await ReadCachedSnapshotAsync(cacheKey, typeCacheKey, cancellationToken);
            if (cached != null) return Result<SnapshotPayload>.Success(cached);
        }

        var camera = await _unitOfWork.Cameras.GetAsync(
            where: c => c.Id == cameraId,
            cancellationToken: cancellationToken);

        if (camera == null)
            return Result<SnapshotPayload>.NotFound(description: "Kamera bulunamadi");

        // TEK UCUS (single-flight): ayni kameranin goruntusunu es zamanli
        // isteyen istemciler kameraya TEK istek uretir. Grid'de 8 kutucuk ayni
        // anda yenilendiginde kilit olmasaydi kamera 8 kez zorlanirdi ve
        // kucuk bir IP kamera bunu kaldiramaz.
        var gate = SnapshotLocks.GetOrAdd(cameraId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            // Kilit beklerken baska biri doldurmus olabilir — cift kontrol.
            if (!fresh)
            {
                var cached = await ReadCachedSnapshotAsync(cacheKey, typeCacheKey, cancellationToken);
                if (cached != null) return Result<SnapshotPayload>.Success(cached);
            }

            var result = await _snapshotGateway.GetSnapshotAsync(camera, cancellationToken);
            if (!result.IsSuccess) return result;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_captureSettings.SnapshotCacheSeconds)
            };
            await _cache.SetAsync(cacheKey, result.Data.Content, options, cancellationToken);
            await _cache.SetStringAsync(typeCacheKey, result.Data.ContentType, options, cancellationToken);

            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<SnapshotPayload?> ReadCachedSnapshotAsync(string cacheKey, string typeCacheKey, CancellationToken cancellationToken)
    {
        byte[]? content = await _cache.GetAsync(cacheKey, cancellationToken);
        if (content == null || content.Length == 0) return null;

        string contentType = await _cache.GetStringAsync(typeCacheKey, cancellationToken) ?? "image/jpeg";
        return new SnapshotPayload(content, contentType);
    }
}
