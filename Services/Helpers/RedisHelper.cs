using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Repositories.Common;
using Services.Interfaces;
using StackExchange.Redis;

namespace Services.Helpers;

public class RedisHelper : IRedisHelper
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDistributedCache _distributedCache;
    private readonly bool _isEnabled;

    public RedisHelper(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer,
        IConfiguration configuration)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
        bool.TryParse(configuration["Redis:IsEnabled"], out var isEnabled);
        _isEnabled = isEnabled;
    }

    public async Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = default,
        TimeSpan? slidingExpiration = default)
    {
        if (_isEnabled)
        {
            // Set default values if they are not provided
            absoluteExpiration ??= TimeSpan.FromMinutes(Constant.DefaultAbsoluteExpirationInMinutes);
            slidingExpiration ??= TimeSpan.FromMinutes(Constant.DefaultSlidingExpirationInMinutes);

            // Cache the data
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            };
            var serializedData = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
            await _distributedCache.SetStringAsync(cacheKey, serializedData, cacheOptions);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> getData,
        TimeSpan? absoluteExpiration = default,
        TimeSpan? slidingExpiration = default)
    {
        if (_isEnabled)
        {
            // Attempt to get data from the cache
            var cachedData = await _distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(cachedData))
                // If data exists, deserialize and return it
                // TODO: cachedData is deserialized to ResponseModel but its Data field is still a JSON object
                return JsonSerializer.Deserialize<T>(cachedData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })!;
        }

        // Data not in cache; retrieve data from the provided function
        var data = await getData();
        if (_isEnabled)
            // Cache the data
            await SetAsync(cacheKey, data, absoluteExpiration, slidingExpiration);

        return data;
    }

    public async Task InvalidateCacheByPatternAsync(string pattern)
    {
        if (_isEnabled)
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints()[0]);
            foreach (var key in server.Keys(pattern: pattern))
                if (key.ToString().Length > 0)
                    await _distributedCache.RemoveAsync(key!);
        }
    }
}