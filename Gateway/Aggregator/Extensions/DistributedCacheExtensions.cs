using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace Aggregator.Extensions;

internal static class DistributedCacheExtensions
{
    public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        where T : class
    {
        var cacheRecord = await cache.GetAsync(key);
        if (cacheRecord is null)
        {
            return null;
        }

        var cacheJson = Encoding.UTF8.GetString(cacheRecord);
        return JsonSerializer.Deserialize<T>(cacheJson);
    }

    public static async Task SetAsync<T>(this IDistributedCache cache,
        string key, T value, DistributedCacheEntryOptions options)
        where T : class
    {
        var valueJson = JsonSerializer.Serialize(value);
        var valueBytes = Encoding.UTF8.GetBytes(valueJson);
        await cache.SetAsync(key, valueBytes, options);
    }
}
