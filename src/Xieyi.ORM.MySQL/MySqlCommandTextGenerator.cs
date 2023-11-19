using System.Linq.Expressions;
using Xieyi.ORM.Core.DbContext;
using Xieyi.ORM.Core.SqlStatementManagement;

namespace Xieyi.ORM.MySQL
{
    public class MySqlCommandTextGenerator : CommandTextGeneratorBase
    {
        public MySqlCommandTextGenerator(SqlDbContext sqlDbContext) : base(sqlDbContext)
        {
        }

        public override void SetWhere<TEntity>(Expression<Func<TEntity, bool>> where)
        {
            throw new NotImplementedException();
        }

        public override void SetOrderBy<TEntity>(Expression<Func<TEntity, object>> orderBy, bool isDesc)
        {
            throw new NotImplementedException();
        }

        public override void SetPage(int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public override void SetLimit(int count)
        {
            throw new NotImplementedException();
        }

        public override void SetAlias(string alias)
        {
            throw new NotImplementedException();
        }

        public override void SetColumns<TEntity>(Expression<Func<TEntity, object>> columns)
        {
            throw new NotImplementedException();
        }

        public override string Add<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override string Update<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public override string Update<TEntity>(TEntity entity, out Expression<Func<TEntity, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public override string Delete<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override string Delete<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public override string QueryableCount<TEntity>()
        {
            throw new NotImplementedException();
        }

        public override string QueryableAny<TEntity>()
        {
            throw new NotImplementedException();
        }

        public override string QueryableQuery<TEntity>()
        {
            throw new NotImplementedException();
        }

        public override string QueryablePaging<TEntity>()
        {
            throw new NotImplementedException();
        }
    }
}