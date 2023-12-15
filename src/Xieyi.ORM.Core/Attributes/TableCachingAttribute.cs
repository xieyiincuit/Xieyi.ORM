namespace Xieyi.ORM.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class TableCachingAttribute : Attribute
{
    public TimeSpan ExpiredTime { get; private set; }

    public TableCachingAttribute()
    {
    }

    public TableCachingAttribute(int expiredTimeMinutes)
    {
        ExpiredTime = TimeSpan.FromMinutes(expiredTimeMinutes);
    }

    public static bool IsExistTaleCaching(Type type, out TimeSpan timeSpan)
    {
        var attr = type.GetCustomAttributes(typeof(TableCachingAttribute), true)?.FirstOrDefault();
        timeSpan = (attr as TableCachingAttribute)?.ExpiredTime ?? TimeSpan.Zero; //这里默认给Zero，在TableCache里面判断Zero则获取Context的默认值
        return attr != null;
    }
}