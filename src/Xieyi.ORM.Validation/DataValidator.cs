using Xieyi.ORM.Core;
using Xieyi.ORM.Validation.Attributes;

namespace Xieyi.ORM.Validation
{
    /// <summary>
    /// 字段值合法性校验器
    /// </summary>
    public class DataValidator : IDataValidator
    {
        public void Verify<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                return;

            foreach (var propertyInfo in typeof(TEntity).GetProperties())
            {
                var value = propertyInfo.GetValue(entity);

                //Require
                RequireAttribute.Verify(propertyInfo, value);

                //StringLength
                StringLengthAttribute.Verify(propertyInfo, value);

                //RangeLimit
                RangeLimitAttribute.Verify(propertyInfo, value);
            }
        }

        public void Verify<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            if (entities == null || !entities.Any())
                return;

            foreach (var propertyInfo in typeof(TEntity).GetProperties())
            {
                foreach (var item in entities)
                {
                    var value = propertyInfo.GetValue(item);

                    //Require
                    RequireAttribute.Verify(propertyInfo, value);

                    //StringLength
                    StringLengthAttribute.Verify(propertyInfo, value);

                    //RangeLimit
                    RangeLimitAttribute.Verify(propertyInfo, value);
                }
            }
        }
    }
}