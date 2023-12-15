namespace Xieyi.ORM.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class TableAttribute : Attribute
{
    public TableAttribute()
    {
    }

    public TableAttribute(string tableName)
    {
        Name = tableName;
    }

    public string Name { get; private set; }

    public static string GetName(Type type)
    {
        var attr = type.GetCustomAttributes(typeof(TableAttribute), true)?.FirstOrDefault();
        return (attr as TableAttribute)?.Name ?? type.Name;
    }
}