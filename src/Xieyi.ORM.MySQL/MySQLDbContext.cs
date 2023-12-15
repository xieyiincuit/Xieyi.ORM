using System.Data.Common;
using MySql.Data.MySqlClient;
using Xieyi.ORM.Core.Attributes;
using Xieyi.ORM.Core.DbContext;
using Xieyi.ORM.Core.Extensions;
using Xieyi.ORM.Core.SqlStatementManagement;

namespace Xieyi.ORM.MySQL;

public abstract class MySQLDbContext<TDataBase> : SqlDbContext, IDisposable where TDataBase : class
{
    protected MySQLDbContext(string connectionString_Write, params string[] connectionStrings_Read) : base(connectionString_Write, connectionStrings_Read)
    {
        DataBaseType = DataBaseType.MySql;
        DataBaseName = DataBaseAttribute.GetName(typeof(TDataBase));
    }

    protected override DbConnection CreateDbConnection(string connectionString)
    {
        return new MySqlConnection(connectionString);
    }

    protected override DbCommand CreateDbCommand()
    {
        return new MySqlCommand(string.Empty, (MySqlConnection)DbConnection);
    }

    protected override DbDataAdapter CreateDbDataAdapter()
    {
        return new MySqlDataAdapter((MySqlCommand)DbCommand);
    }

    protected override CommandTextGeneratorBase CreateCommandTextGenerator()
    {
        return new MySqlCommandTextGenerator(this);
    }

    internal override void ParameterInitializes()
    {
        if (Parameters == null || !Parameters.Any()) return;

        DbCommand.Parameters.Clear();
        Parameters.Foreach(t => DbCommand.Parameters.Add(new MySqlParameter(t.Key, t.Value ?? DBNull.Value)));
    }

    internal override List<TEntity> GetFullCollectionData<TEntity>()
    {
        using (var db = new MySQLTableCacheDbContext(ConnectionManager.ConnectionString_Write, ConnectionManager.ConnectionStrings_Read))
        {
            db.RealExecutionSaveToDb = RealExecutionSaveToDb;

            db.DataBaseName = DataBaseName;
            db.SqlStatement = $"SELECT * FROM {CollectionName}";
            return db.QueryExecutor.ExecuteList<TEntity>();
        }
    }

    public new void Dispose()
    {
        base.Dispose();
    }
}

internal class MySQLTableCacheDbContext : MySQLDbContext<MySQLTableCacheDbContext>
{
    public MySQLTableCacheDbContext(string connectionString_Write, params string[] connectionStrings_Read) : base(connectionString_Write, connectionStrings_Read)
    {
    }
}