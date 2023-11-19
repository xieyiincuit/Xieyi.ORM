using System.Linq.Expressions;
using Xieyi.ORM.Core.Attributes;
using Xieyi.ORM.Core.DbContext;

namespace Xieyi.ORM.Core.QueryEngine;

internal abstract class SqlQueryableBase<TEntity> where TEntity : class
{
    protected SqlDbContext _dbContext { get; }

    protected SqlQueryableBase(SqlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string SqlStatement => _dbContext.DbCommand.CommandText;
    public string TableName => _dbContext.TableName;
    public IDictionary<string, object> Parameters => _dbContext.Parameters;

    protected Expression<Func<TEntity, bool>> _where = t => true;
    
    protected Expression<Func<TEntity, object>> _orderby;
    protected bool _isDesc = false;

    protected bool _isPaging = false;
    protected int _pageIndex = 0;
    protected int _pageSize = 0;

    /// <summary>
    /// 要查询的列
    /// </summary>
    protected Expression<Func<TEntity, object>> _columns;

    /// <summary>
    /// 必要条件检查
    /// </summary>
    protected void MustExistCheck()
    {
        if (_where == null)
            throw new ArgumentNullException(nameof(_where));
    }

    /// <summary>
    /// 获取TableName，并将其重新赋值
    /// </summary>
    protected void ReSetTableName()
    {
        _dbContext.TableName = TableAttribute.GetName(typeof(TEntity));
    }
}