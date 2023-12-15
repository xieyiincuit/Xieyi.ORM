using System.Linq.Expressions;

namespace Xieyi.ORM.Core
{
    public interface ICacheManager
    {
        void Add<TEntity>(TEntity entity) where TEntity : class;
        void Add<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        void Update<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> filter) where TEntity : class;
        void Delete<TEntity>(TEntity entity) where TEntity : class;
        TEntity GetEntity<TEntity>(Expression<Func<TEntity, bool>> filter, Func<TEntity> queryFunc) where TEntity : class;
        List<TEntity> GetEntities<TEntity>(Expression<Func<TEntity, bool>> filter, Func<List<TEntity>> queryFunc) where TEntity : class;
        long GetCount<TEntity>(Expression<Func<TEntity, bool>> filter, Func<long> queryFunc) where TEntity : class;

        void FlushAllCache();
        void FlushCurrentCollectionCache(string collectionName = null);
    }
}