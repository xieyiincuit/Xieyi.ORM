namespace Xieyi.ORM.Core.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ColumnIgnoreAttribute : Attribute
{
    public ColumnIgnoreAttribute()
    {
    }
}