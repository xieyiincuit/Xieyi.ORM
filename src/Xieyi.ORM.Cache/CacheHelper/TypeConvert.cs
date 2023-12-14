using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace Xieyi.ORM.Cache
{
    internal class TypeConvert
    {
        public static T ToGenericType<T>(object value)
        {
            if (value == null)
            {
                return default(T);
            }

            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //声明一个NullableConverter类，该类提供从Nullable类到基础基元类型的转换
                var nullableConverter = new NullableConverter(type);
                return (T)Convert.ChangeType(value, nullableConverter.UnderlyingType);
            }
            else if (type.IsClass)
            {
                return value switch
                {
                    //复杂类型Json反序列化不彻底，需要二次处理
                    JObject jObject => jObject.ToObject<T>(),
                    JArray array => array.ToObject<T>(),
                    _ => (T)value
                };
            }
            else
            {
                return (T)Convert.ChangeType(value, type);
            }
        }
    }
}