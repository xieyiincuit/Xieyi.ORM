using System.Data.Common;
using Xieyi.ORM.Core.DbContext;
using Xieyi.ORM.Core.SqlStatementManagement;

namespace Xieyi.ORM.MySQL
{
    public abstract class MySQLDbContext<TDataBase> : SqlDbContext, IDisposable where TDataBase : class
    {
        protected MySQLDbContext(string connectionString_Write, params string[] connectionStrings_Read) : base(connectionString_Write, connectionStrings_Read)
        {
        }

        protected override DbConnection CreateDbConnection(string connectionString)
        {
            throw new NotImplementedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            throw new NotImplementedException();
        }

        protected override DbDataAdapter CreateDbDataAdapter()
        {
            throw new NotImplementedException();
        }

        protected override CommandTextGeneratorBase CreateCommandTextGenerator()
        {
            throw new NotImplementedException();
        }
    }
}