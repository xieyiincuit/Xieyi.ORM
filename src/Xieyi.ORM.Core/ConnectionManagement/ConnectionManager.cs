using Xieyi.ORM.Core.Extensions;

namespace Xieyi.ORM.Core.ConnectionManagement;

public class ConnectionManager
{
    private readonly IList<ConnectionStatus> connectionStatuses = null;

    internal ConnectionManager(string connectionString_Write, string[] connectionStrings_Read)
    {
        ConnectionString_Write = connectionString_Write;
        ConnectionStrings_Read = connectionStrings_Read?.Distinct().ToArray();

        CurrentConnectionString = connectionString_Write;

        if (ConnectionStrings_Read == null || !ConnectionStrings_Read.Any() || ConnectionStrings_Read.Length == 1)
            return;

        connectionStatuses = new List<ConnectionStatus>();

        //初始化连接池使用情况集合
        ConnectionStrings_Read.Distinct().ToArray().Foreach(item => connectionStatuses.Add(new ConnectionStatus { HashKey = item.GetHashCode(), ConnectionString = item, Count = 0 }));
    }

    public string CurrentConnectionString { get; private set; }

    public string NextConnectionString { get; private set; }

    public string ConnectionString_Write { get; private set; }

    public string[] ConnectionStrings_Read { get; private set; }

    public LoadBalanceStrategy ConnectionLoadBalanceStrategy { get; protected set; } = LoadBalanceStrategy.LeastConnection;

    /// <summary>
    /// 设置连接字符串
    /// </summary>
    /// <param name="operationType"></param>
    /// <returns></returns>
    internal string SetAndGetConnectionString(OperationType operationType)
    {
        if (operationType == OperationType.Write)
        {
            CurrentConnectionString = ConnectionString_Write;
            return CurrentConnectionString;
        }

        //先校验下次执行的连接字符串
        if (!string.IsNullOrEmpty(NextConnectionString))
        {
            CurrentConnectionString = NextConnectionString;
            NextConnectionString = string.Empty;
            return CurrentConnectionString;
        }

        if (ConnectionStrings_Read == null || !ConnectionStrings_Read.Any())
        {
            CurrentConnectionString = ConnectionString_Write;
        }
        else if (ConnectionStrings_Read.Length == 1)
        {
            CurrentConnectionString = ConnectionStrings_Read.First();
        }
        else
        {
            if (connectionStatuses == null)
                throw new NullReferenceException("Connection status list is null, please call Init() first!");

            //根据策略选取对应的连接字符串
            CurrentConnectionString = ConnectionLoadBalanceStrategy switch
            {
                LoadBalanceStrategy.RoundRobin => GetByRoundRobin(),
                LoadBalanceStrategy.LeastConnection => LeastConnection(),
                _ => LeastConnection()
            };
        }

        return CurrentConnectionString;
    }

    /// <summary>
    /// 轮询获取
    /// </summary>
    /// <returns></returns>
    private string GetByRoundRobin()
    {
        var current = connectionStatuses.FirstOrDefault(t => t.HashKey == CurrentConnectionString.GetHashCode());
        if (current == null)
            throw new KeyNotFoundException("current connection not fount in connection strings, please check the connection list");

        //获取当前元素索引
        var currentIndex = connectionStatuses.IndexOf(current);

        return currentIndex < connectionStatuses.Count
            ? connectionStatuses.ElementAt(currentIndex + 1).ConnectionString
            : connectionStatuses.First().ConnectionString;
    }

    /// <summary>
    /// 最小连接获取
    /// </summary>
    /// <returns></returns>
    private string LeastConnection()
    {
        var current = connectionStatuses.OrderBy(t => t.Count).First();
        current.Count++;
        return current.ConnectionString;
    }
}