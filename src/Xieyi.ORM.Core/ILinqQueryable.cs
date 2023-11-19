using System.Linq.Expressions;

namespace Xieyi.ORM.Core
{
    public interface ILinqQueryable<TEntity> : IQueryable<TEntity>
    {
        ILinqQueryable<TEntity> Where(Expression<Func<TEntity, bool>> filter);
        ILinqQueryable<TEntity> OrderBy(Expression<Func<TEntity, object>> orderBy);
        ILinqQueryable<TEntity> OrderByDescending(Expression<Func<TEntity, object>> orderBy);
        ILinqQueryable<TEntity> Paging(int pageIndex, int pageSize);
        ILinqQueryable<TEntity> Select(Expression<Func<TEntity, object>> columns);
        ILinqQueryable<TEntity> Limit(int count);
        long Count();
        bool Any();
    }
}