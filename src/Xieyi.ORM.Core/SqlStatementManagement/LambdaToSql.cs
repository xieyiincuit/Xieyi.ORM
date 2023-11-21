using System.Linq.Expressions;
using System.Text;
using Xieyi.ORM.Core.Attributes;
using Xieyi.ORM.Core.Extensions;

namespace Xieyi.ORM.Core.SqlStatementManagement
{
    internal class LambdaToSql
    {
        public static string ConvertWhere<T>(Expression<Func<T, bool>> where) where T : class
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            var builder = new StringBuilder();
            builder.Append(" WHERE ");
            if (where.Body is BinaryExpression be)
            {
                return builder.Append(BinaryExpressionProvider(be.Left, be.Right, be.NodeType, ref parameters)).ToString();
            }

            return builder.Append(ExpressionRouter(where.Body, ref parameters)).ToString();
        }

        public static string ConvertWhere<T>(Expression<Func<T, bool>> where, out IDictionary<string, object> parameters) where T : class
        {
            parameters = new Dictionary<string, object>();

            var builder = new StringBuilder();
            builder.Append(" WHERE ");
            if (where.Body is BinaryExpression be)
            {
                return builder.Append(BinaryExpressionProvider(be.Left, be.Right, be.NodeType, ref parameters)).ToString();
            }

            return builder.Append(ExpressionRouter(where.Body, ref parameters)).ToString();
        }

        public static string ConvertOrderBy<T>(Expression<Func<T, object>> orderBy) where T : class
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            if (orderBy.Body is UnaryExpression ue)
            {
                return ExpressionRouter(ue.Operand, ref parameters);
            }
            else
            {
                var order = (MemberExpression)orderBy.Body;
                return order.Member.Name;
            }
        }

        //转换查询列
        public static List<string> ConvertColumns<TEntity>(Expression<Func<TEntity, object>> columns) where TEntity : class
        {
            if (columns == null)
            {
                return null;
            }

            var strList = new List<string>();
            if (columns.Body is NewExpression newExp)
            {
                strList.AddRange(newExp.Arguments.Select(GetFieldAttribute));
            }
            else
            {
                strList.Add(GetFieldAttribute(columns.Body));
            }

            return strList;
        }

        //通过Attribute获取需要查找的字段列表
        private static string GetFieldAttribute(Expression exp)
        {
            switch (exp)
            {
                case UnaryExpression expression:
                {
                    var ue = expression;
                    return GetFieldAttribute(ue.Operand);
                }
                case MemberExpression expression:
                {
                    var mem = expression;
                    var member = mem.Member;
                    var metaFieldAttr = member.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault();
                    var metaFieldName = (metaFieldAttr as ColumnAttribute)?.Name ?? member.Name;
                    return metaFieldName;
                }
                default:
                {
                    throw new ArgumentException("Unsupported Expression Type, Pls use [UnaryExpression] or [MemberExpression]");
                }
            }
        }

        private static string BinaryExpressionProvider(Expression left, Expression right, ExpressionType type, ref IDictionary<string, object> parameters)
        {
            var leftValue = ExpressionRouter(left, ref parameters);
            var typeCast = ExpressionTypeCast(type);
            var rightValue = ExpressionRouter(right, ref parameters);

            if (left is MemberExpression && (right is ConstantExpression || right is MemberExpression))
            {
                var keyNameNoPoint = leftValue.Replace(".", "");

                keyNameNoPoint = FindAppropriateKey($"@{keyNameNoPoint}", parameters);

                parameters.AddOrUpdate(keyNameNoPoint, $"{rightValue}");

                return $"{leftValue} {typeCast} {keyNameNoPoint}";
            }
            else
            {
                return $"({leftValue}) {typeCast} ({rightValue})";
            }
        }

        private static string ExpressionRouter(Expression exp, ref IDictionary<string, object> parameters)
        {
            switch (exp)
            {
                case BinaryExpression be:
                    return BinaryExpressionProvider(be.Left, be.Right, be.NodeType, ref parameters);
                case MemberExpression me when !exp.ToString().StartsWith("value"):
                    return me.ToString();
                case MemberExpression:
                {
                    var result = Expression.Lambda(exp).Compile().DynamicInvoke();
                    switch (result)
                    {
                        case null:
                            return "NULL";
                        case Guid:
                            return $"{result}";
                        case ValueType:
                            return result.ToString();
                        default:
                        {
                            if (result is string || result is DateTime || result is char)
                                return $"'{result}'";
                            break;
                        }
                    }

                    break;
                }
                case NewArrayExpression ae:
                {
                    var sBuilder = new StringBuilder();
                    foreach (var ex in ae.Expressions)
                    {
                        sBuilder.Append(ExpressionRouter(ex, ref parameters));
                        sBuilder.Append(",");
                    }

                    return sBuilder.ToString(0, sBuilder.Length - 1);
                }
                case MethodCallExpression mce:
                {
                    var value = mce.Object == null ? Expression.Lambda(mce).Compile().DynamicInvoke().ToString() : Expression.Lambda(mce.Arguments[0]).Compile().DynamicInvoke().ToString();

                    var keyName = mce.Object?.ToString();
                    var keyNameNoPoint = keyName?.Replace(".", "");

                    keyNameNoPoint = FindAppropriateKey($"@{keyNameNoPoint}", parameters);

                    switch (mce.Method.Name)
                    {
                        case "Equals":
                            parameters.AddOrUpdate(keyNameNoPoint, $"{value.Replace("'", "")}");
                            return $"{keyName} = {keyNameNoPoint}";
                        case "Contains":
                            parameters.AddOrUpdate(keyNameNoPoint, $"%{value.Replace("'", "")}%");
                            return $"{keyName} LIKE {keyNameNoPoint}";
                        case "StartsWith":
                            parameters.AddOrUpdate(keyNameNoPoint, $"{value.Replace("'", "")}%");
                            return $"{keyName} LIKE {keyNameNoPoint}";
                        case "EndsWith":
                            parameters.AddOrUpdate(keyNameNoPoint, $"%{value.Replace("'", "")}");
                            return $"{keyName} LIKE {keyNameNoPoint}";
                        default:
                            return value;
                    }
                }
                case ConstantExpression ce when ce.Value == null:
                    return "NULL";
                case ConstantExpression ce when ce.Value is ValueType:
                {
                    if (ce.Value is bool b)
                    {
                        if (b)
                            return " 1=1 ";
                        else
                            return " 1=2 ";
                    }

                    return ce.Value.ToString();
                }
                case ConstantExpression ce when ce.Value is string || ce.Value is DateTime || ce.Value is char:
                    return $"'{ce.Value}'";
                case UnaryExpression ue:
                    return ExpressionRouter(ue.Operand, ref parameters);
            }

            return null;
        }

        private static string ExpressionTypeCast(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                    return " AND ";
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                    return " Or ";
                case ExpressionType.OrElse:
                    return " Or ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.MultiplyChecked:
                    return "*";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 处理同key但是不同value的情况,查找适合的key返回
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parameters"></param>
        private static string FindAppropriateKey(string key, IDictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey(key))
            {
                return key;
            }

            //循环99次，如果期间有符合条件的直接返回
            for (int i = 0; i < 99; i++)
            {
                string tempKey = $"{key}{i}";
                if (!parameters.ContainsKey(tempKey))
                {
                    return tempKey;
                }
            }

            throw new KeyNotFoundException($"The appropriate key was not found in the interval [{key}0,{key}99]");
        }
    }
}