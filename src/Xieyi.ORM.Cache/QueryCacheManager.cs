using Xieyi.ORM.Core.DbContext;
using Xieyi.ORM.Core.Extensions;

namespace Xieyi.ORM.Cache
{
    /// <summary>
    /// 查询缓存管理器（一级缓存管理器）
    /// QueryCache存储结构，以表为缓存单位，便于在对单个表进行操作以后释放单个表的缓存，每个表的缓存以hash字典的方式存储
    /// Key-CollectionName
    /// Value-Dictionary<string, T>
    ///       {
    ///            sql.HashCode(),值
    ///       }
    /// </summary>
    internal class QueryCacheManager : CacheManagerBase
    {
        internal QueryCacheManager(DbContext context, CacheOptions cacheOptions) : base(context, cacheOptions)
        {
        }

        /// <summary>
        /// SQL查询语句缓存（细粒度-每个表中可能有多个SQL语句缓存）
        /// </summary>
        /// <returns></returns>
        private string GetSqlQueryCacheKey() => DbContext.GetQueryCacheKey();

        /// <summary>
        /// SQL查询语句缓存Key（粗粒度-数据库中的表）
        /// </summary>
        /// <returns></returns>
        private string GetQueryCacheKey(string collectionName = null)
        {
            string key = $"{CacheOptionsConst.CacheKey_QueryCache}{collectionName ?? DbContext.CollectionName}";

            #region Keys Set 维护（对应不同数据库中的所有集合CacheKey）

            if (!CacheManager.IsExist(CacheOptionsConst.GetQueryCacheKeysCacheKey(DbContext.DataBaseName), out HashSet<string> keys))
                keys = new HashSet<string>();

            keys.Add(key);

            CacheManager.Put(CacheOptionsConst.GetQueryCacheKeysCacheKey(DbContext.DataBaseName), keys, CacheOptions.MaxExpiredTimeSpan);

            #endregion

            return key;
        }

        /// <summary>
        /// QueryCache级别存储
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheValue"></param>
        internal void SetCacheData<T>(T cacheValue)
        {
            string queryCacheKey = GetQueryCacheKey();
            string sqlQueryCacheKey = GetSqlQueryCacheKey();

            if (CacheManager.IsExist(queryCacheKey, out Dictionary<string, object> t))
            {
                //如果超出单表的query缓存键阈值，则按先后顺序进行移除
                if (t.Count >= CacheOptions.QueryCacheMaxCountPerTable)
                    t.Remove(t.First().Key);

                t.AddOrUpdate(sqlQueryCacheKey, cacheValue);
                CacheManager.Put(queryCacheKey, t, CacheOptions.QueryCacheExpiredTimeSpan);
            }
            else
            {
                //如果缓存中没有表单位的缓存，则直接新增表单位的sql键缓存
                var dic = new Dictionary<string, object> { { sqlQueryCacheKey, cacheValue } };
                CacheManager.Put(queryCacheKey, dic, CacheOptions.QueryCacheExpiredTimeSpan);
            }
        }

        /// <summary>
        /// 从一级缓存中获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal T GetEntitiesFromCache<T>()
        {
            if (CacheManager.IsExist(GetQueryCacheKey(), out Dictionary<string, object> dic))
            {
                var sqlCacheKey = GetSqlQueryCacheKey();
                if (dic.ContainsKey(sqlCacheKey))
                {
                    DbContext.IsFromCache = true;
                    return TypeConvert.ToGenericType<T>(dic[sqlCacheKey]);
                }
            }

            return default(T);
        }

        /// <summary>
        /// 清空单个表相关的所有缓存
        /// </summary>
        /// <param name="dbContext"></param>
        internal void FlushCollectionCache(string collectionName = null)
        {
            CacheManager.Delete(GetQueryCacheKey(collectionName));
        }

        /// <summary>
        /// 清空数据库中所有缓存
        /// </summary>
        internal void FlushAllCache()
        {
            if (CacheManager.IsExist(CacheOptionsConst.GetQueryCacheKeysCacheKey(DbContext.DataBaseName), out HashSet<string> keys))
            {
                foreach (var item in keys)
                {
                    CacheManager.Delete(item);
                }
            }
        }
    }
}