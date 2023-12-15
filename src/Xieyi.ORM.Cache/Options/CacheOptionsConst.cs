namespace Xieyi.ORM.Cache
{
    public class CacheOptionsConst
    {
        /// <summary>
        /// 每张表一级缓存的最大个数，超出数目将会按从早到晚的顺序移除缓存键
        /// </summary>
        internal static readonly int QueryCacheMaxCountPerTable = 50;

        /// <summary>
        /// QueryCache默认过期时间
        /// </summary>
        internal static readonly TimeSpan QueryCacheExpiredTimeSpan = TimeSpan.FromMinutes(10);

        /// <summary>
        /// TableCache默认过期时间
        /// </summary>
        internal static readonly TimeSpan TableCacheExpiredTimeSpan = TimeSpan.FromHours(6);

        /// <summary>
        /// QueryCache前缀
        /// </summary>
        internal const string CacheKey_QueryCache = "QueryCache__";

        /// <summary>
        /// TableCache前缀
        /// </summary>
        internal const string CacheKey_TableCache = "TableCache_";

        /// <summary>
        /// 表扫描key前缀
        /// </summary>
        internal const string CacheKey_TableScanning = "CacheScaning_";

        /// <summary>
        /// TableCache扫描键最多存在时间
        /// </summary>
        internal static readonly TimeSpan SpanScanningKeyExpiredTime = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Query缓存键缓存的key前缀(用于保存所有的QueryCache keys)
        /// </summary>
        internal const string CacheKey_QueryCacheKeys = "QueryCacheKeys__";

        /// <summary>
        /// Table缓存键缓存的key前缀(用于保存所有的TableCache keys)
        /// </summary>
        internal const string CacheKey_TableCacheKeys = "TableCacheKeys__";
        
        /// <summary>
        /// 缓存键缓存的最大时间，该值只是个默认时间，保证在该配置中最大集合，实际动态计算为最大时间
        /// </summary>
        internal static readonly TimeSpan CacheKeysMaxExpiredTime = TimeSpan.FromDays(1);

        internal static string GetQueryCacheKeysCacheKey(string dataBaseName) => $"{CacheKey_QueryCacheKeys}{dataBaseName}";

        internal static string GetTableCacheKeysCacheKey(string dataBaseName) => $"{CacheKey_TableCacheKeys}{dataBaseName}";
    }
}