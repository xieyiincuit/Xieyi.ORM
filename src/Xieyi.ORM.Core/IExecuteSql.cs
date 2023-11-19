namespace Xieyi.ORM.Core
{
    public interface IExecuteSql
    {
        int ExecuteSql(string sql, IDictionary<string, object> parameters = null);
        Task<int> ExecuteSqlAsync(string sql, IDictionary<string, object> parameters = null);

        int ExecuteProcedure(string procedureName, IDictionary<string, object> parameters = null);
        Task<int> ExecuteProcedureAsync(string procedureName, IDictionary<string, object> parameters = null);
    }
}