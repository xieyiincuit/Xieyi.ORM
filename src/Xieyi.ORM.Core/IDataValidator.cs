namespace Xieyi.ORM.Core
{
    public interface IDataValidator
    {
        void Verify<TEntity>(TEntity entity) where TEntity : class;
        void Verify<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    }
}