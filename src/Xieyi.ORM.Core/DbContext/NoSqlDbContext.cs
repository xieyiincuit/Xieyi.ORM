namespace Xieyi.ORM.Core.DbContext;

public abstract class NoSqlDbContext : DbContext
{
    protected NoSqlDbContext(string connectionString_Write, params string[] connectionStrings_Read) : base(connectionString_Write, connectionStrings_Read)
    {
    }

    internal string QueryCacheKey { get; set; }

    internal override string GetQueryCacheKey()
    {
        return QueryCacheKey;
    }
}