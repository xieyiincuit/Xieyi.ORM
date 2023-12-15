namespace Xieyi.ORM.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class DataBaseAttribute : Attribute
{
    public DataBaseAttribute()
    {
    }

    public DataBaseAttribute(string databaseName)
    {
        Name = databaseName;
    }

    public string Name { get; private set; }

    public static string GetName(Type type)
    {
        var attr = type.GetCustomAttributes(typeof(DataBaseAttribute), true)?.FirstOrDefault();
        return (attr as DataBaseAttribute)?.Name ?? type.Name;
    }
}