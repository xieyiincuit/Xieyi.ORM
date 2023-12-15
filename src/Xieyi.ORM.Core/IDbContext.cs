using System.Linq.Expressions;

namespace Xieyi.ORM.Core;

public interface IDbContext : IDisposable
{
    void Add<TEntity>(TEntity entity) where TEntity : class;
    Task AddAsync<TEntity>(TEntity entity) where TEntity : class;

    void Update<TEntity>(Expression<Func<TEntity, bool>> filter, TEntity entity) where TEntity : class;
    Task UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> filter, TEntity entity) where TEntity : class;

    void Delete<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;
    Task DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;
}