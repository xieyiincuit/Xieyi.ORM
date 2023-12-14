using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Xieyi.ORM.Validation.Attributes
{
    /// <summary>
    /// 是否必填
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RequireAttribute : ValidationAttribute
    {
        public RequireAttribute(string errorMsg = null) : base(errorMsg ?? string.Empty)
        {
        }

        internal static void Verify(PropertyInfo propertyInfo, object value)
        {
            if (propertyInfo.GetCustomAttribute(typeof(RequireAttribute), true) is RequireAttribute require)
            {
                if (value == null)
                    throw new ArgumentNullException(require.ErrorMessage ?? $"value of '{propertyInfo.Name}' can not be null");
            }
        }
    }
}