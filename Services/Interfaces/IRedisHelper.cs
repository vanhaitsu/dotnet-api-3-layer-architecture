namespace Services.Interfaces;

public interface IRedisHelper
{
    Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> getData, TimeSpan? absoluteExpiration = default,
        TimeSpan? slidingExpiration = default);

    Task SetAsync<T>(string cacheKey, T data, TimeSpan? absoluteExpiration = default,
        TimeSpan? slidingExpiration = default);

    Task InvalidateCacheByPatternAsync(string pattern);
}