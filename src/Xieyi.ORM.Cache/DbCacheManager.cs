using System.Linq.Expressions;
using Xieyi.ORM.Core;
using Xieyi.ORM.Core.DbContext;

namespace Xieyi.ORM.Cache
{
    public class DbCacheManager : CacheManagerBase, ICacheManager
    {
        internal QueryCacheManager QueryCacheManager { get; private set; }
        internal TableCacheManager TableCacheManager { get; private set; }
        
        public DbCacheManager(DbContext context, CacheOptions cacheOptions) : base(context, cacheOptions)
        {
            if (context == null) throw new ArgumentException(nameof(context));
            if (cacheOptions == null) throw new ArgumentException(nameof(cacheOptions));
            
            if (cacheOptions.OpenQueryCache)
                QueryCacheManager = new QueryCacheManager(context, cacheOptions);
            if (cacheOptions.OpenTableCache)
                TableCacheManager = new TableCacheManager(context, cacheOptions);
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (CacheOptions.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache();
            
            if (CacheOptions.OpenTableCache)
                TableCacheManager.AddCache(entity);
        }

        public void Add<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            if (CacheOptions.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache();
            
            if (CacheOptions.OpenTableCache)
                TableCacheManager.AddCache(entities);
        }

        public void Update<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> filter) where TEntity : class
        {
            if (CacheOptions.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache();
            
            if (CacheOptions.OpenTableCache)
                TableCacheManager.UpdateCache(entity, filter);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            if (CacheOptions.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache();
            
            if (CacheOptions.OpenTableCache)
                TableCacheManager.DeleteCache(entity);
        }

        public void Delete<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            if (CacheOptions.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache();
            
            if (CacheOptions.OpenTableCache)
                TableCacheManager.DeleteCache(filter);
        }
        
        public TEntity GetEntity<TEntity>(Expression<Func<TEntity, bool>> filter, Func<TEntity> queryFunc) where TEntity : class
        {
            DbContext.IsFromCache = false;

            TEntity result = null;

            //Get Cache From TableCache
            if (CacheOptions.OpenTableCache)
                result = TableCacheManager.GetEntitiesFromCache(filter)?.FirstOrDefault();
            if (DbContext.IsFromCache)
                return result;
            
            //Get Cache From QueryCache
            if (CacheOptions.OpenQueryCache)
                result = QueryCacheManager.GetEntitiesFromCache<TEntity>();
            if (DbContext.IsFromCache)
                return result;
            
            //Get Cache From Database
            result = queryFunc();

            if (CacheOptions.OpenQueryCache)
                QueryCacheManager.SetCacheData(result);

            return result;
        }

        public List<TEntity> GetEntities<TEntity>(Expression<Func<TEntity, bool>> filter, Func<List<TEntity>> queryFunc) where TEntity : class
        {
            DbContext.IsFromCache = false;

            List<TEntity> result = null;

            //Get Cache From TableCache
            if (CacheOptions.OpenTableCache)
                result = TableCacheManager.GetEntitiesFromCache(filter);
            if (DbContext.IsFromCache)
                return result;
            
            //Get Cache From QueryCache
            if (CacheOptions.OpenQueryCache)
                result = QueryCacheManager.GetEntitiesFromCache<List<TEntity>>();
            if (DbContext.IsFromCache)
                return result;
            
            //Get Cache From Database
            result = queryFunc();

            if (CacheOptions.OpenQueryCache)
                QueryCacheManager.SetCacheData(result);

            return result;
        }

        public long GetCount<TEntity>(Expression<Func<TEntity, bool>> filter, Func<long> queryFunc) where TEntity : class
        {
            DbContext.IsFromCache = false;

            long? result = null;

            if (CacheOptions.OpenTableCache)
                result = TableCacheManager.GetEntitiesFromCache(filter)?.Count;

            if (DbContext.IsFromCache)
                return result ?? default(long);

            if (CacheOptions.OpenQueryCache)
                result = QueryCacheManager.GetEntitiesFromCache<long?>();

            if (DbContext.IsFromCache)
                return result ?? default(long);

            result = queryFunc();

            if (CacheOptions.OpenQueryCache)
                QueryCacheManager.SetCacheData(result);

            return result ?? default(long);
        }

        public void FlushAllCache()
        {
            if (CacheOptions.OpenQueryCache)
                QueryCacheManager.FlushAllCache();
            
            if (CacheOptions.OpenTableCache)
                TableCacheManager.FlushAllCache();
        }

        public void FlushCurrentCollectionCache(string collectionName = null)
        {
            if (CacheOptions.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache(collectionName);
            
            if (CacheOptions.OpenTableCache)
                TableCacheManager.FlushCollectionCache(collectionName);
        }
    }
}