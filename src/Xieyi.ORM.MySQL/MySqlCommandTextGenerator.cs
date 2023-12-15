using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Xieyi.ORM.Core.Attributes;
using Xieyi.ORM.Core.DbContext;
using Xieyi.ORM.Core.Exceptions;
using Xieyi.ORM.Core.Extensions;
using Xieyi.ORM.Core.SqlStatementManagement;

namespace Xieyi.ORM.MySQL;

public class MySqlCommandTextGenerator : CommandTextGeneratorBase
{
    public MySqlCommandTextGenerator(SqlDbContext sqlDbContext) : base(sqlDbContext)
    {
    }

    public override void SetWhere<TEntity>(Expression<Func<TEntity, bool>> where)
    {
        _where = LambdaToSql.ConvertWhere(where, out var parameters);
        SqlDbContext.Parameters = parameters;
    }

    public override void SetOrderBy<TEntity>(Expression<Func<TEntity, object>> orderBy, bool isDesc)
    {
        if (orderBy == null)
            return;
        var desc = isDesc ? "DESC" : "ASC";
        _orderBy = $" ORDER BY {LambdaToSql.ConvertOrderBy(orderBy)} {desc}".TrimEnd();
    }

    public override void SetPage(int pageIndex, int pageSize)
    {
        _pageIndex = pageIndex;
        _pageSize = pageSize;
    }

    public override void SetLimit(int count)
    {
        _limit = $"LIMIT {count}";
    }

    public override void SetAlias(string alias)
    {
        _alias = alias;
    }

    public override void SetColumns<TEntity>(Expression<Func<TEntity, object>> columns)
    {
        _columns = LambdaToSql.ConvertColumns<TEntity>(columns);
    }

    public override string Add<TEntity>(TEntity entity)
    {
        SqlDbContext.Parameters = new Dictionary<string, object>();
        SqlDbContext.TableName = TableAttribute.GetName(typeof(TEntity));

        StringBuilder builder_front = new(), builder_behind = new();
        builder_front.Append("INSERT INTO ");
        builder_front.Append(SqlDbContext.TableName);
        builder_front.Append(" (");
        builder_behind.Append(" VALUES (");

        var propertyInfos = GetPropertiesDicByType(typeof(TEntity));
        foreach (var propertyInfo in propertyInfos)
        {
            if (propertyInfo.GetCustomAttribute(typeof(AutoIncreaseAttribute), true) is AutoIncreaseAttribute)
            {
            }
            else if (propertyInfo.GetCustomAttribute(typeof(ColumnAttribute), true) is ColumnAttribute columnAttribute)
            {
                builder_front.Append(columnAttribute.GetName(propertyInfo.Name));
                builder_front.Append(",");

                var columnName = columnAttribute.GetName(propertyInfo.Name).Replace("`", "");
                builder_behind.Append("@");
                builder_behind.Append(columnName);
                builder_behind.Append(",");

                SqlDbContext.Parameters.AddOrUpdate($"@{columnName}", propertyInfo.GetValue(entity));
            }

            //in the end,remove the redundant symbol of ','
            if (propertyInfos.Last() == propertyInfo)
            {
                builder_front.Remove(builder_front.Length - 1, 1);
                builder_front.Append(")");

                builder_behind.Remove(builder_behind.Length - 1, 1);
                builder_behind.Append(")");
            }
        }

        return SqlDbContext.SqlStatement = builder_front.Append(builder_behind.ToString()).ToString().TrimEnd();
    }

    public override string Update<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> filter)
    {
        SqlDbContext.Parameters = new Dictionary<string, object>();
        SqlDbContext.TableName = TableAttribute.GetName(typeof(TEntity));

        var builder_front = new StringBuilder();
        builder_front.Append("UPDATE ");
        builder_front.Append(SqlDbContext.TableName);
        builder_front.Append(" ");

        var alias = filter.Parameters[0].Name;
        builder_front.Append(alias);
        builder_front.Append(" SET ");

        var propertyInfos = GetPropertiesDicByType(typeof(TEntity));
        foreach (var propertyInfo in propertyInfos)
        {
            if (propertyInfo.GetCustomAttribute(typeof(AutoIncreaseAttribute), true) is AutoIncreaseAttribute)
            {
            }
            else if (propertyInfo.GetCustomAttribute(typeof(ColumnAttribute), true) is ColumnAttribute columnAttribute)
            {
                builder_front.Append(columnAttribute.GetName(propertyInfo.Name));
                builder_front.Append("=");
                builder_front.Append($"@{alias}");
                var columnName = columnAttribute.GetName(propertyInfo.Name).Replace("`", "");
                builder_front.Append(columnName);
                builder_front.Append(",");

                SqlDbContext.Parameters.AddOrUpdate($"@{alias}{columnName}", propertyInfo.GetValue(entity));
            }

            //in the end,remove the redundant symbol of ','
            if (propertyInfos.Last() == propertyInfo) builder_front.Remove(builder_front.Length - 1, 1);
        }

        return SqlDbContext.SqlStatement = builder_front.Append($"{LambdaToSql.ConvertWhere(filter)}").ToString().TrimEnd();
    }

    public override string Update<TEntity>(TEntity entity, out Expression<Func<TEntity, bool>> filter)
    {
        SqlDbContext.Parameters = new Dictionary<string, object>();
        SqlDbContext.TableName = TableAttribute.GetName(typeof(TEntity));
        PropertyInfo[] propertyInfos = GetPropertiesDicByType(typeof(TEntity));

        //查找Entity的主键以及主键对应的值，如果用该方法更新数据，主键是必须存在的
        var keyProperty = propertyInfos.FirstOrDefault(t => t.GetCustomAttribute(typeof(KeyAttribute), true) is KeyAttribute);
        if (keyProperty == null)
            throw new TableKeyNotFoundException($"table '{SqlDbContext.TableName}' not found primary key column");

        var keyName = keyProperty.Name;
        var keyValue = keyProperty.GetValue(entity);

        if (keyProperty.GetCustomAttribute(typeof(ColumnAttribute), true) is ColumnAttribute columnAttr)
            keyName = columnAttr.GetName(keyProperty.Name);

        //Generate Expression of update via key : t => t.Key == value
        ParameterExpression param = Expression.Parameter(typeof(TEntity), "t");
        MemberExpression left = Expression.Property(param, keyProperty);
        ConstantExpression right = Expression.Constant(keyValue);
        BinaryExpression where = Expression.Equal(left, right);

        filter = Expression.Lambda<Func<TEntity, bool>>(where, param);

        //将主键的查询参数加到字典中
        SqlDbContext.Parameters.AddOrUpdate($"@t{keyName}", keyValue);

        //开始构造赋值的sql语句
        StringBuilder builder_front = new StringBuilder();
        builder_front.Append("UPDATE ");

        //Mysql和sqlserver的分别处理
        if (SqlDbContext.DataBaseType == DataBaseType.MySql)
        {
            builder_front.Append(SqlDbContext.TableName);
            builder_front.Append(" ");
        }

        //查询语句中表的别名，例如“t”
        string alias = filter.Parameters[0].Name;
        builder_front.Append(alias);
        builder_front.Append(" SET ");

        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            if (propertyInfo.GetCustomAttribute(typeof(AutoIncreaseAttribute), true) is AutoIncreaseAttribute)
            {
            }
            else if (propertyInfo.GetCustomAttribute(typeof(ColumnAttribute), true) is ColumnAttribute columnAttr1)
            {
                builder_front.Append(columnAttr1.GetName(propertyInfo.Name));
                builder_front.Append("=");
                builder_front.Append($"@{alias}");
                var columnName = columnAttr1.GetName(propertyInfo.Name).Replace("`", "");
                builder_front.Append(columnName);
                builder_front.Append(",");

                SqlDbContext.Parameters.AddOrUpdate($"@{alias}{columnName}", propertyInfo.GetValue(entity));
            }

            //in the end,remove the redundant symbol of ','
            if (propertyInfos.Last() == propertyInfo)
            {
                builder_front.Remove(builder_front.Length - 1, 1);
            }
        }

        //SqlServer和Mysql的sql语句分别处理
        if (SqlDbContext.DataBaseType == DataBaseType.SqlServer)
        {
            builder_front.Append(" FROM ");
            builder_front.Append(SqlDbContext.TableName);
            builder_front.Append(" ");
            builder_front.Append(alias);
        }

        return SqlDbContext.SqlStatement = builder_front.Append($"{LambdaToSql.ConvertWhere(filter)}").ToString().TrimEnd();
    }

    public override string Delete<TEntity>(TEntity entity)
    {
        SqlDbContext.Parameters = new Dictionary<string, object>();
        SqlDbContext.TableName = TableAttribute.GetName(typeof(TEntity));
        var propertyInfos = GetPropertiesDicByType(typeof(TEntity));

        //删除时必须要有列或主键列
        var mainKeyProperty = propertyInfos.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
        if (mainKeyProperty == null) throw new TableKeyNotFoundException($"table '{SqlDbContext.TableName}' not found key column");

        var columnName = mainKeyProperty.Name;
        var columnValue = mainKeyProperty.GetValue(entity);

        if (mainKeyProperty.GetCustomAttribute(typeof(ColumnAttribute), true) is ColumnAttribute columnAttr)
            columnName = columnAttr.GetName(mainKeyProperty.Name);

        SqlDbContext.Parameters.AddOrUpdate($"@t{columnName}", columnValue);
        return SqlDbContext.SqlStatement = $"DELETE t FROM {SqlDbContext.TableName} t WHERE t.{columnName} = @t{columnName}".TrimEnd();
    }

    public override string Delete<TEntity>(Expression<Func<TEntity, bool>> filter)
    {
        IDictionary<string, object> parameters;
        SqlDbContext.TableName = TableAttribute.GetName(typeof(TEntity));
        SqlDbContext.SqlStatement = $"DELETE {filter.Parameters[0].Name} From {SqlDbContext.TableName} {filter.Parameters[0].Name} {LambdaToSql.ConvertWhere(filter, out parameters)}".TrimEnd();
        SqlDbContext.Parameters = parameters;
        return SqlDbContext.SqlStatement;
    }

    public override string QueryableCount<TEntity>()
    {
        return SqlDbContext.SqlStatement = $"SELECT COUNT(0) FROM {SqlDbContext.TableName} {_alias} {_where}".TrimEnd();
    }

    public override string QueryableAny<TEntity>()
    {
        SetLimit(1);
        return SqlDbContext.SqlStatement = $"SELECT 1 FROM {SqlDbContext.TableName} {_alias} {_where} {_limit}".TrimEnd();
    }

    public override string QueryableQuery<TEntity>()
    {
        var queryColumns = _columns == null || !_columns.Any() ? "*" : string.Join(",", _columns.Select(t => $"{_alias}.{t}"));
        return SqlDbContext.SqlStatement = $"SELECT {queryColumns} FROM {SqlDbContext.TableName} {_alias} {_where} {_orderBy} {_limit}".TrimEnd();
    }

    public override string QueryablePaging<TEntity>()
    {
        var queryColumns = _columns == null || !_columns.Any() ? "*" : string.Join(",", _columns.Select(t => $"{_alias}.{t}").ToArray());
        return SqlDbContext.SqlStatement = $"SELECT {queryColumns} FROM {SqlDbContext.TableName} {_alias} {_where} {_orderBy} LIMIT {_pageIndex * _pageSize},{_pageSize}".TrimEnd();
    }
}