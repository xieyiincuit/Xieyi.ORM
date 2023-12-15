using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Xieyi.ORM.Validation.Attributes;

/// <summary>
/// String property length limit
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class StringLengthAttribute : ValidationAttribute
{
    internal int MinLength { get; set; } = int.MinValue;
    internal int MaxLength { get; set; }

    public StringLengthAttribute(int maxLength, string errorMsg = null) : base(errorMsg ?? string.Empty)
    {
        MaxLength = maxLength;
    }

    public StringLengthAttribute(int minLength, int maxLength, string errorMsg = null) : base(errorMsg ?? string.Empty)
    {
        MinLength = minLength;
        MaxLength = maxLength;
    }

    internal static void Verify(PropertyInfo propertyInfo, object value)
    {
        if (propertyInfo.GetCustomAttribute(typeof(StringLengthAttribute), true) is StringLengthAttribute stringLength)
        {
            if (propertyInfo.PropertyType != typeof(string))
                throw new CustomAttributeFormatException($"'{nameof(StringLengthAttribute)}' cannot be used in '{propertyInfo.PropertyType}' type property");

            if (value is string strValue && (strValue?.Length > stringLength.MaxLength || strValue?.Length < stringLength.MinLength))
                throw new ArgumentOutOfRangeException(stringLength.ErrorMessage ?? $"value of '{propertyInfo.Name}' is out of range,parameter value:{value}");
        }
    }
}