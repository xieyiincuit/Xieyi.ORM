using Xieyi.ORM.Core.DbContext;

namespace Xieyi.ORM.Cache;

/// <summary>
/// 缓存管理器扩展
/// </summary>
public static class DbCacheManagerExtensions
{
    /// <summary>
    /// 初始化本地缓存
    /// </summary>
    /// <param name="dbContext">数据操作上下文</param>
    /// <param name="openQueryCache">是否开启一级缓存</param>
    /// <param name="openTableCache">是否开启二级缓存</param>
    /// <param name="queryCacheExpiredTimeSpan">一级缓存过期时间</param>
    /// <param name="tableCacheExpiredTimeSpan">二级缓存过期时间</param>
    public static void OpenLocalCache(this DbContext dbContext, bool openQueryCache = false, bool openTableCache = false, TimeSpan queryCacheExpiredTimeSpan = default, TimeSpan tableCacheExpiredTimeSpan = default)
    {
        dbContext.OpenCache(new DbCacheManager(dbContext,
            new CacheOptions()
            {
                OpenQueryCache = openQueryCache,
                OpenTableCache = openTableCache,
                QueryCacheExpiredTimeSpan = queryCacheExpiredTimeSpan,
                TableCacheExpiredTimeSpan = tableCacheExpiredTimeSpan
            }));
    }

    /// <summary>
    /// 初始化Redis缓存
    /// </summary>
    /// <param name="dbContext">数据操作上下文</param>
    /// <param name="openQueryCache">是否开启一级缓存</param>
    /// <param name="openTableCache">是否开启二级缓存</param>
    /// <param name="cacheServer">Redis address</param>
    public static void OpenRedisCache(this DbContext dbContext, string cacheServer, bool openQueryCache = false, bool openTableCache = false)
    {
        dbContext.OpenCache(new DbCacheManager(dbContext,
            new CacheOptions()
            {
                OpenQueryCache = openQueryCache,
                OpenTableCache = openTableCache,
                CacheMediaType = CacheMediaType.Redis,
                CacheMediaServer = cacheServer
            }));
    }
}