using System.Data;
using System.Linq.Expressions;
using Xieyi.ORM.Core.Extensions;
using Xieyi.ORM.Core.DbContext;

namespace Xieyi.ORM.Core.QueryEngine
{
    internal class SqlQueryable<TEntity> : SqlQueryableBase<TEntity>, ILinqQueryable<TEntity> where TEntity : class
    {
        private string Alias => _where.Parameters[0].Name;

        public SqlQueryable(SqlDbContext dbContext) : base(dbContext)
        {
        }

        public object ToData()
        {
            if (_dbContext.IsSqlStatementOrStoredProcedure)
                return _dbContext.QueryExecutor.ExecuteScalar();

            MustExistCheck();
            ReSetTableName();

            _dbContext.CommandTextGenerator.SetAlias(Alias);
            _dbContext.CommandTextGenerator.SetColumns(_columns);
            _dbContext.CommandTextGenerator.SetWhere(_where);
            _dbContext.CommandTextGenerator.SetOrderBy(_orderby, _isDesc);
            _dbContext.CommandTextGenerator.SetLimit(1);
            _dbContext.CommandTextGenerator.QueryableQuery<TEntity>();

            return _dbContext.QueryExecutor.ExecuteScalar();
        }

        public DataSet ToDataSet()
        {
            if (_dbContext.IsSqlStatementOrStoredProcedure)
                return _dbContext.QueryExecutor.ExecuteDataSet();

            MustExistCheck();
            ReSetTableName();

            _dbContext.CommandTextGenerator.SetAlias(Alias);
            _dbContext.CommandTextGenerator.SetColumns(_columns);
            _dbContext.CommandTextGenerator.SetWhere(_where);
            _dbContext.CommandTextGenerator.SetOrderBy(_orderby, _isDesc);

            if (_isPaging)
            {
                _dbContext.CommandTextGenerator.SetPage(_pageIndex, _pageSize);
                _dbContext.CommandTextGenerator.QueryablePaging<TEntity>();
            }
            else
            {
                _dbContext.CommandTextGenerator.QueryableQuery<TEntity>();
            }

            return _dbContext.QueryExecutor.ExecuteDataSet();
        }

        public List<TEntity> ToList()
        {
            if (_dbContext.IsSqlStatementOrStoredProcedure)
                return _dbContext.QueryExecutor.ExecuteList<TEntity>();

            MustExistCheck();
            ReSetTableName();

            _dbContext.CommandTextGenerator.SetAlias(Alias);
            _dbContext.CommandTextGenerator.SetColumns(_columns);
            _dbContext.CommandTextGenerator.SetWhere(_where);
            _dbContext.CommandTextGenerator.SetOrderBy(_orderby, _isDesc);

            if (_isPaging)
            {
                _dbContext.CommandTextGenerator.SetPage(_pageIndex, _pageSize);
                _dbContext.CommandTextGenerator.QueryablePaging<TEntity>();
            }
            else
            {
                _dbContext.CommandTextGenerator.QueryableQuery<TEntity>();
            }

            return _dbContext.DbCacheManagerSafeExecute((m, r) => m.GetEntities(_where, r), () => _dbContext.QueryExecutor.ExecuteList<TEntity>());
        }

        public TEntity FirstOrDefault()
        {
            if (_dbContext.IsSqlStatementOrStoredProcedure)
                return _dbContext.QueryExecutor.ExecuteEntity<TEntity>();

            MustExistCheck();
            ReSetTableName();

            _dbContext.CommandTextGenerator.SetAlias(Alias);
            _dbContext.CommandTextGenerator.SetColumns(_columns);
            _dbContext.CommandTextGenerator.SetWhere(_where);
            _dbContext.CommandTextGenerator.SetOrderBy(_orderby, _isDesc);
            _dbContext.CommandTextGenerator.SetLimit(1);

            _dbContext.CommandTextGenerator.QueryableQuery<TEntity>();

            return _dbContext.DbCacheManagerSafeExecute((m, r) => m.GetEntity(_where, r), () => _dbContext.QueryExecutor.ExecuteEntity<TEntity>());
        }

        public long Count()
        {
            MustExistCheck();
            ReSetTableName();

            _dbContext.CommandTextGenerator.SetAlias(Alias);
            _dbContext.CommandTextGenerator.SetWhere(_where);
            _dbContext.CommandTextGenerator.QueryableCount<TEntity>();

            return _dbContext.DbCacheManagerSafeExecute((m, r) => m.GetCount(_where, r), () => Convert.ToInt64(_dbContext.QueryExecutor.ExecuteScalar()));
        }

        public bool Any()
        {
            MustExistCheck();
            ReSetTableName();

            _dbContext.CommandTextGenerator.SetAlias(Alias);
            _dbContext.CommandTextGenerator.SetWhere(_where);
            _dbContext.CommandTextGenerator.QueryableCount<TEntity>();

            return _dbContext.DbCacheManagerSafeExecute((m, r) => m.GetCount(_where, r), () => Convert.ToInt64(_dbContext.QueryExecutor.ExecuteScalar())) > 0;
        }

        public ILinqQueryable<TEntity> Where(Expression<Func<TEntity, bool>> filter)
        {
            if (filter != null)
                _where = _where.And(filter);

            return this;
        }

        public ILinqQueryable<TEntity> OrderBy(Expression<Func<TEntity, object>> orderBy)
        {
            if (orderBy != null)
            {
                _orderby = orderBy;
                _isDesc = false;
            }

            return this;
        }

        public ILinqQueryable<TEntity> OrderByDescending(Expression<Func<TEntity, object>> orderBy)
        {
            if (orderBy != null)
            {
                _orderby = orderBy;
                _isDesc = true;
            }

            return this;
        }

        public ILinqQueryable<TEntity> Paging(int pageIndex, int pageSize)
        {
            _isPaging = true;

            if (pageIndex <= 0)
                pageIndex = 0;

            if (pageSize <= 0)
                pageSize = 10;

            _pageIndex = pageIndex;
            _pageSize = pageSize;
            return this;
        }

        public ILinqQueryable<TEntity> Select(Expression<Func<TEntity, object>> columns)
        {
            _columns = columns;
            return this;
        }

        public ILinqQueryable<TEntity> Limit(int count)
        {
            _dbContext.CommandTextGenerator.SetLimit(count);
            return this;
        }
    }
}