namespace Xieyi.ORM.Validation.Attributes
{
    /// <summary>
    /// value data range limit, apply to IsValueType like: int,double,float,datetime,decimal...
    /// </summary>
    public class MaxLimitAttribute : RangeLimitAttribute
    {
        public MaxLimitAttribute(double maxValue, string errorMsg = null) : base(maxValue: maxValue, errorMsg: errorMsg)
        {
        }
    }
}