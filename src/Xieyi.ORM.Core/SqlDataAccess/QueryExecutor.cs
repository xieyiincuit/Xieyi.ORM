using System.Data;
using System.Data.Common;
using Xieyi.ORM.Core.DbContext;

namespace Xieyi.ORM.Core.SqlDataAccess;

internal class QueryExecutor
{
    internal SqlDbContext DbContext { get; }

    internal QueryExecutor(SqlDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public int ExecuteNonQuery()
    {
        if (!DbContext.RealExecutionSaveToDb) return default;

        DbContext.ParameterInitializes();
        DbContext.CheckConnectionStatus();
        return DbContext.DbCommand.ExecuteNonQuery();
    }

    public async Task<int> ExecuteNonQueryAsync()
    {
        if (!DbContext.RealExecutionSaveToDb) return default;

        DbContext.ParameterInitializes();
        DbContext.CheckConnectionStatus();
        return await DbContext.DbCommand.ExecuteNonQueryAsync();
    }

    public object ExecuteScalar()
    {
        if (!DbContext.RealExecutionSaveToDb) return default;

        DbContext.ParameterInitializes();
        DbContext.CheckConnectionStatus();
        return DbContext.DbCommand.ExecuteScalar();
    }

    public async Task<object> ExecuteScalarAsync()
    {
        if (!DbContext.RealExecutionSaveToDb) return default;

        DbContext.ParameterInitializes();
        DbContext.CheckConnectionStatus();
        return await DbContext.DbCommand.ExecuteScalarAsync();
    }

    public DbDataReader ExecuteReader()
    {
        if (!DbContext.RealExecutionSaveToDb) return default;
        DbContext.ParameterInitializes();
        DbContext.CheckConnectionStatus();

        return DbContext.DbCommand.ExecuteReader(CommandBehavior.CloseConnection);
    }

    public async Task<DbDataReader> ExecuteReaderAsync()
    {
        if (!DbContext.RealExecutionSaveToDb) return default;
        DbContext.ParameterInitializes();
        DbContext.CheckConnectionStatus();

        return await DbContext.DbCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection);
    }

    public DataTable ExecuteDataTable()
    {
        if (!DbContext.RealExecutionSaveToDb) return default;

        var ds = ExecuteDataSet();
        if (ds != null && ds.Tables.Count > 0) return ds.Tables[0];

        return default;
    }

    public DataSet ExecuteDataSet()
    {
        if (!DbContext.RealExecutionSaveToDb) return default;

        DbContext.ParameterInitializes();
        DbContext.CheckConnectionStatus();

        var ds = new DataSet();
        DbContext.DbDataAdapter.Fill(ds);
        return ds;
    }

    public List<Entity> ExecuteList<Entity>() where Entity : class
    {
        return GetListFromDataSetV2<Entity>(ExecuteDataSet());
    }

    public Entity ExecuteEntity<Entity>() where Entity : class
    {
        return GetEntityFromDataSetV2<Entity>(ExecuteDataSet());
    }

    public List<Entity> GetListFromDataSetV2<Entity>(DataSet ds) where Entity : class
    {
        if (!DbContext.RealExecutionSaveToDb) return default;

        var dt = ds.Tables[0];
        if (dt.Rows.Count <= 0) return default;

        var list = new List<Entity>();
        foreach (DataRow row in dt.Rows)
        {
            var entity = FillAdapter<Entity>.AutoFill(row);
            list.Add(entity);
        }

        return list;
    }

    public Entity GetEntityFromDataSetV2<Entity>(DataSet ds) where Entity : class
    {
        if (!DbContext.RealExecutionSaveToDb) return default;
        var dt = ds.Tables[0]; // 获取到ds的dt
        return dt.Rows.Count > 0 ? FillAdapter<Entity>.AutoFill(dt.Rows[0]) : default;
    }
}