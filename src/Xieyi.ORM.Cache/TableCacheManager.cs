using System.Linq.Expressions;
using Xieyi.ORM.Core.Attributes;
using Xieyi.ORM.Core.DbContext;

namespace Xieyi.ORM.Cache;

internal class TableCacheManager : CacheManagerBase
{
    public TableCacheManager(DbContext context, CacheOptions cacheOptions) : base(context, cacheOptions)
    {
    }

    private string GetTableCacheKey(string collectionName = null)
    {
        var key = $"{CacheOptionsConst.CacheKey_TableCache}_{DbContext.DataBaseName}_{collectionName ?? DbContext.CollectionName}";

        //缓存键更新
        if (!CacheManager.IsExist(CacheOptionsConst.GetTableCacheKeysCacheKey(DbContext.DataBaseName), out HashSet<string> keys))
            keys = new HashSet<string>();

        keys.Add(key);

        CacheManager.Put(CacheOptionsConst.GetTableCacheKeysCacheKey(DbContext.DataBaseName), keys, CacheOptions.MaxExpiredTimeSpan);

        return key;
    }

    public void FlushAllCache()
    {
        if (CacheManager.IsExist(CacheOptionsConst.GetTableCacheKeysCacheKey(DbContext.DataBaseName), out HashSet<string> keys))
            foreach (var item in keys)
                CacheManager.Delete(item);
    }

    /// <summary>
    /// 清空单个表相关的所有缓存
    /// </summary>
    /// <param name="dbContext"></param>
    public void FlushCollectionCache(string collectionName = null)
    {
        CacheManager.Delete(GetTableCacheKey(collectionName));
    }

    internal void AddCache<TEntity>(TEntity entity)
    {
        var tableName = TableAttribute.GetName(typeof(TEntity));
        //如果存在表级别缓存，则更新数据到缓存
        if (CacheManager.IsExist(GetTableCacheKey(tableName), out List<TEntity> entities))
            if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out var tableCacheTimeSpan))
            {
                entities.Add(entity);
                //如果过期时间为0，则取上下文的过期时间
                CacheManager.Put(GetTableCacheKey(tableName), entities, tableCacheTimeSpan == TimeSpan.Zero ? CacheOptions.TableCacheExpiredTimeSpan : tableCacheTimeSpan);
            }
    }

    internal void AddCache<TEntity>(IEnumerable<TEntity> values)
    {
        var tableName = TableAttribute.GetName(typeof(TEntity));
        //如果存在表级别缓存，则更新数据到缓存
        if (CacheManager.IsExist(GetTableCacheKey(tableName), out List<TEntity> entities))
            if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out var tableCacheTimeSpan))
            {
                entities.AddRange(values);
                //如果过期时间为0，则取上下文的过期时间
                CacheManager.Put(GetTableCacheKey(tableName), entities, tableCacheTimeSpan == TimeSpan.Zero ? CacheOptions.TableCacheExpiredTimeSpan : tableCacheTimeSpan);
            }
    }

    internal void UpdateCache<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> updateFilter)
    {
        if (updateFilter == null)
            throw new ArgumentException(nameof(updateFilter));

        var tableName = TableAttribute.GetName(typeof(TEntity));
        if (CacheManager.IsExist(GetTableCacheKey(tableName), out List<TEntity> entities))
            if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out var tableCacheTimeSpan))
            {
                var needUpdateEntity = entities.Where(updateFilter.Compile()).FirstOrDefault();
                if (needUpdateEntity != null)
                {
                    needUpdateEntity = entity;
                    CacheManager.Put(GetTableCacheKey(tableName), entities, tableCacheTimeSpan == TimeSpan.Zero ? CacheOptions.TableCacheExpiredTimeSpan : tableCacheTimeSpan);
                }
            }
    }

    internal void DeleteCache<TEntity>(TEntity entity)
    {
        var tableName = TableAttribute.GetName(typeof(TEntity));

        if (CacheManager.IsExist(GetTableCacheKey(tableName), out List<TEntity> entities))
            if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out var tableCacheTimeSpan))
            {
                var val = entities.Find(t => t.Equals(entity));
                if (val != null)
                {
                    entities.Remove(val);
                    CacheManager.Put(GetTableCacheKey(tableName), entities, tableCacheTimeSpan == TimeSpan.Zero ? CacheOptions.TableCacheExpiredTimeSpan : tableCacheTimeSpan);
                }
            }
    }

    internal void DeleteCache<TEntity>(Expression<Func<TEntity, bool>> deleteFilter)
    {
        if (deleteFilter == null)
            throw new ArgumentException(nameof(deleteFilter));

        var tableName = TableAttribute.GetName(typeof(TEntity));
        //如果存在表级别缓存，则更新数据到缓存
        if (CacheManager.IsExist(GetTableCacheKey(tableName), out List<TEntity> entities))
            if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out var tableCacheTimeSpan))
            {
                //从缓存集合中寻找该记录，如果找到，则更新该记录
                var list = entities.Where(deleteFilter.Compile()).ToList();
                if (list.Any())
                {
                    entities.RemoveAll(t => list.Contains(t));
                    //如果过期时间为0，则取上下文的过期时间
                    CacheManager.Put(GetTableCacheKey(tableName), entities, tableCacheTimeSpan == TimeSpan.Zero ? CacheOptions.TableCacheExpiredTimeSpan : tableCacheTimeSpan);
                }
            }
    }

    internal List<TEntity> GetEntitiesFromCache<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
    {
        if (filter == null)
            throw new ArgumentException(nameof(filter));

        if (CacheManager.IsExist(GetTableCacheKey(TableAttribute.GetName(typeof(TEntity))), out List<TEntity> entities))
        {
            DbContext.IsFromCache = true;
            return entities.Where(filter.Compile()).ToList();
        }

        if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out var tableCacheTimeSpan)) StartScanTable<TEntity>(tableCacheTimeSpan);

        return null;
    }

    private readonly object _lockObj = new();

    private void StartScanTable<TEntity>(TimeSpan tableCacheTimeSpan) where TEntity : class
    {
        var scanKey = $"{CacheOptionsConst.CacheKey_TableScanning}{DbContext.CollectionName}";
        if (CacheManager.IsExist(scanKey))
            return;

        CacheManager.Put(scanKey, 1, CacheOptionsConst.SpanScanningKeyExpiredTime);

        Task.Run(() =>
        {
            lock (_lockObj)
            {
                var tableName = TableAttribute.GetName(typeof(TEntity));
                if (CacheManager.IsExist(GetTableCacheKey(tableName)))
                    return;

                var data = DbContext.GetFullCollectionData<TEntity>();

                CacheManager.Put(GetTableCacheKey(tableName), data ?? new List<TEntity>(), tableCacheTimeSpan == TimeSpan.Zero ? CacheOptions.TableCacheExpiredTimeSpan : tableCacheTimeSpan);
            }

            CacheManager.Delete(scanKey);
        });
    }
}