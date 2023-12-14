namespace Xieyi.ORM.Validation.Attributes
{
    /// <summary>
    /// value data range limit,apply to IsValueType like: int,double,float,datetime,decimal...
    /// </summary>
    public class MinLimitAttribute : RangeLimitAttribute
    {
        public MinLimitAttribute(double minValue, string errorMsg = null) : base(minValue: minValue, errorMsg: errorMsg)
        {
        }
    }
}