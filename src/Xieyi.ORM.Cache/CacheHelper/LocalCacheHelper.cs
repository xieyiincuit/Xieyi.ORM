using Microsoft.Extensions.Caching.Memory;

namespace Xieyi.ORM.Cache;

internal class LocalCacheHelper
{
    private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public static TValue Put<TKey, TValue>(TKey key, TValue value)
    {
        return _cache.Set(key, value);
    }

    public static TValue Put<TKey, TValue>(TKey key, TValue value, TimeSpan absoluteExpirationRelativeToNow)
    {
        return _cache.Set(key, value, absoluteExpirationRelativeToNow);
    }

    public static TValue Put<TKey, TValue>(TKey key, TValue value, DateTime absoluteExpiration)
    {
        return _cache.Set(key, value, absoluteExpiration - DateTime.Now);
    }

    public static TValue Get<TKey, TValue>(TKey key)
    {
        if (Exist(key)) return _cache.Get<TValue>(key);

        return default;
    }

    public static bool Exist<TKey>(TKey key)
    {
        return _cache.TryGetValue(key, out var value);
    }

    public static bool Exist<TKey, TValue>(TKey key, out TValue value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public static void Delete<TKey>(TKey key)
    {
        _cache.Remove(key);
    }
}