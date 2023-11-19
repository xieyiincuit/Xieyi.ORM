namespace Xieyi.ORM.Core.ConnectionManagement
{
    internal class ConnectionStatus
    {
        public int HashKey { get; set; }
        public string ConnectionString { get; set; }
        public int Count { get; set; }
    }
}