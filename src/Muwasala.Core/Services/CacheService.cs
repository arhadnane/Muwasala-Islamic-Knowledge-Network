using System.Collections.Concurrent;

namespace Muwasala.Core.Services;

/// <summary>
/// Simple in-memory cache service for performance optimization
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;
    Task RemoveAsync(string key);
    Task ClearAsync();
}

/// <summary>
/// In-memory cache implementation with expiration
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
    private readonly Timer _cleanupTimer;

    public MemoryCacheService()
    {
        // Cleanup expired items every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out var item) && item.Expiration > DateTime.UtcNow)
        {
            return Task.FromResult(item.Value as T);
        }

        // Remove expired item
        if (item != null)
        {
            _cache.TryRemove(key, out _);
        }

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        var cacheItem = new CacheItem(value, DateTime.UtcNow.Add(expiration));
        _cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _cache.Clear();
        return Task.CompletedTask;
    }

    private void CleanupExpiredItems(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.Expiration <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }

    private record CacheItem(object Value, DateTime Expiration);

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}
