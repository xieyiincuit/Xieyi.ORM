using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Xieyi.ORM.Core.Extensions;
using Xieyi.ORM.Core.DbContext;

namespace Xieyi.ORM.Core.SqlStatementManagement;

public abstract class CommandTextGeneratorBase
{
    protected SqlDbContext SqlDbContext { get; }

    protected CommandTextGeneratorBase(SqlDbContext sqlDbContext)
    {
        SqlDbContext = sqlDbContext;
    }

    protected List<string> _columns;
    protected string _alias;
    protected string _where;
    protected string _orderBy;
    protected int _pageIndex;
    protected int _pageSize;
    protected string _limit;

    //Cache properties by type
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertiesDic = new();

    protected static PropertyInfo[] GetPropertiesDicByType(Type type)
    {
        _propertiesDic.AddOrUpdate(type, type.GetProperties());
        return _propertiesDic[type];
    }

    public abstract void SetWhere<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class;
    public abstract void SetOrderBy<TEntity>(Expression<Func<TEntity, object>> orderBy, bool isDesc) where TEntity : class;
    public abstract void SetPage(int pageIndex, int pageSize);
    public abstract void SetLimit(int count);
    public abstract void SetAlias(string alias);
    public abstract void SetColumns<TEntity>(Expression<Func<TEntity, object>> columns) where TEntity : class;

    public abstract string Add<TEntity>(TEntity entity) where TEntity : class;
    public abstract string Update<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> filter) where TEntity : class;
    public abstract string Update<TEntity>(TEntity entity, out Expression<Func<TEntity, bool>> filter) where TEntity : class;
    public abstract string Delete<TEntity>(TEntity entity) where TEntity : class;
    public abstract string Delete<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;

    public abstract string QueryableCount<TEntity>() where TEntity : class;
    public abstract string QueryableAny<TEntity>() where TEntity : class;
    public abstract string QueryableQuery<TEntity>() where TEntity : class;
    public abstract string QueryablePaging<TEntity>() where TEntity : class;
}