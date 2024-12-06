namespace Services.Interfaces;

public interface IRedisHelper
{
    Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = default,
        TimeSpan? slidingExpiration = default);

    Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> getData, TimeSpan? absoluteExpiration = default,
        TimeSpan? slidingExpiration = default);

    Task InvalidateCacheByPatternAsync(string pattern);
}