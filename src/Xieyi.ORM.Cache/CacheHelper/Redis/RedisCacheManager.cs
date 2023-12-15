namespace Xieyi.ORM.Cache;

internal class RedisCacheManager : RedisServerManager, IRedisCache
{
    private TimeSpan AbsoluteExpirationRelativeToNow { get; set; }

    public RedisCacheManager(string server = "localhost:6379", int defaultExpireSecond = 1800) : base("ORM", server)
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(defaultExpireSecond);
    }

    public string Get(string key)
    {
        return Db.StringGet(key);
    }

    public void Set(string key, string value)
    {
        Db.StringSet(key, value, AbsoluteExpirationRelativeToNow);
    }

    public void Set(string key, string value, TimeSpan absoluteExpirationRelativeToNow)
    {
        Db.StringSet(key, value, absoluteExpirationRelativeToNow);
    }

    public void Set(string key, string value, DateTime absoluteExpiration)
    {
        Db.StringSet(key, value, absoluteExpiration - DateTime.Now);
    }

    public void Update(string key, string value)
    {
        Db.StringSet(key, value);
    }

    public void Delete(string key)
    {
        Db.KeyDelete(key);
    }

    public bool Exist(string key)
    {
        return Db.KeyExists(key);
    }

    public long StringIncrement(string key)
    {
        return Db.StringIncrement(key);
    }

    public double StringIncrement(string key, double incrementNumber)
    {
        return Db.StringIncrement(key, incrementNumber);
    }
}