using CabinetOs.Core.Utils.ResultPattern;

namespace CabinetOs.Core.Utils.Caching;

public interface ICacheService
{
    Result<string> Get(string cacheKey);
    Result Add<TData>(string cacheKey, TData data, string[]? cacheGroupKeys = default);
    Result Remove(string cacheKey);
    Result RemoveCacheGroups(string[] cacheGroupKeys);
}