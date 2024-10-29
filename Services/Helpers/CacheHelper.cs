using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.Common;
using Services.Interfaces;
using StackExchange.Redis;

namespace Services.Helpers;

public class CacheHelper : ICacheHelper
{
    private readonly IConfiguration _configuration;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDistributedCache _distributedCache;

    public CacheHelper(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer,
        IConfiguration configuration)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
        _configuration = configuration;
        Enable = bool.Parse(_configuration["Redis:Enable"] ?? "false");
    }

    private bool Enable { get; }

    public async Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> getData,
        TimeSpan? absoluteExpiration = default,
        TimeSpan? slidingExpiration = default)
    {
        if (Enable)
        {
            // Attempt to get data from the cache
            var cachedData = await _distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
                // If data exists, deserialize and return it
                return JsonConvert.DeserializeObject<T>(cachedData)!;
        }

        // Data not in cache; retrieve data from the provided function
        var data = await getData();

        if (Enable)
            // Cache the data
            await SetAsync(cacheKey, data, absoluteExpiration, slidingExpiration);

        return data;
    }

    public async Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = default,
        TimeSpan? slidingExpiration = default)
    {
        if (Enable)
        {
            // Set default values if they are not provided
            absoluteExpiration ??= TimeSpan.FromMinutes(Constant.DEFAULT_ABSOLUTE_EXPIRATION_IN_MINUTES);
            slidingExpiration ??= TimeSpan.FromMinutes(Constant.DEFAULT_SLIDING_EXPIRATION_IN_MINUTES);

            // Cache the data
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            };
            var serializedData = JsonConvert.SerializeObject(data);
            await _distributedCache.SetStringAsync(cacheKey, serializedData, cacheOptions);
        }
    }

    public async Task InvalidateCacheByPatternAsync(string pattern)
    {
        if (Enable)
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints()[0]);
            foreach (var key in server.Keys(pattern: pattern))
                if (key.ToString().Length > 0)
                    await _distributedCache.RemoveAsync(key!);
        }
    }
}