using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Xieyi.ORM.Validation.Attributes
{
    /// <summary>
    /// value data range limit,apply to IsValueType like: int,double,float,datetime,decimal...
    /// </summary>
    public class RangeLimitAttribute : ValidationAttribute
    {
        internal double MinValue { get; set; }
        internal double MaxValue { get; set; }

        public RangeLimitAttribute(double minValue = double.MinValue, double maxValue = double.MaxValue, string errorMsg = null) : base(errorMsg ?? string.Empty)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

        internal static void Verify(PropertyInfo propertyInfo, object value)
        {
            if (propertyInfo.GetCustomAttribute(typeof(RangeLimitAttribute), true) is RangeLimitAttribute rangeLimit)
            {
                if (!propertyInfo.PropertyType.IsValueType)
                    throw new CustomAttributeFormatException($"'{nameof(RangeLimitAttribute)}' cannot be used in an unValueType property like '{propertyInfo.PropertyType}'");

                if (value == null)
                    throw new ArgumentNullException(rangeLimit.ErrorMessage ?? $"value of '{propertyInfo.Name}' can not be null");

                double val = Convert.ToDouble(value);

                if (val > rangeLimit.MaxValue || val < rangeLimit.MinValue)
                    throw new ArgumentOutOfRangeException(rangeLimit.ErrorMessage ?? $"value of '{propertyInfo.Name}' is out of range:[{rangeLimit.MinValue},{rangeLimit.MaxValue}]，parameter value:{value}");
            }
        }
    }
}