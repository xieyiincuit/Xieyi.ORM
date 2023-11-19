using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Xieyi.ORM.Core.Attributes;

namespace Xieyi.ORM.Core.SqlDataAccess
{
    internal class FillAdapter<Entity>
    {
        private static readonly Func<DataRow, Entity> funcCache = GetFactory();

        public static Entity AutoFill(DataRow row)
        {
            return funcCache(row);
        }

        private static Func<DataRow, Entity> GetFactory()
        {
            var type = typeof(Entity);
            var rowType = typeof(DataRow);
            var rowDeclare = Expression.Parameter(rowType, "row");
            var instanceDeclare = Expression.Parameter(type, "t");

            var newExpression = Expression.New(type);
            var instanceExpression = Expression.Assign(instanceDeclare, newExpression);

            var notNullEqualExpression = Expression.NotEqual(rowDeclare, Expression.Constant(null));

            var containsMethod = typeof(DataColumnCollection).GetMethod("Contains");
            var indexerMethod = rowType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, new[] { new ParameterModifier(1) });
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var setExpressions = new List<Expression>();
            var columns = Expression.Property(Expression.Property(rowDeclare, "Table"), "Columns");

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.GetCustomAttribute(typeof(ColumnAttribute), true) == null)
                    continue;
                if (!propertyInfo.CanWrite)
                    continue;

                //Id, Id is a property of Entity
                var propertyName = Expression.Constant(propertyInfo.Name, typeof(string));
                //row.Table.Columns.Contains("Id")
                var checkIfContainsColumn = Expression.Call(columns, containsMethod!, propertyName);
                //t.Id
                var propertyExpression = Expression.Property(instanceDeclare, propertyInfo);
                //row.get_Item("Id")
                var value = Expression.Call(rowDeclare, indexerMethod!, propertyName);
                //t.Id = Convert(row.get_Item("Id"), Int32)
                var propertyAssign = Expression.Assign(propertyExpression, Expression.Convert(value, propertyInfo.PropertyType));
                //t.Id = default(Int32)
                var propertyAssignDefault = Expression.Assign(propertyExpression, Expression.Default(propertyInfo.PropertyType));
                //if (row.Table.Columns.Contains("Id") && !value.Equals(DBNull.Value<>)) { t.Id = Convert(row.get_Item("Id"), Int32) } else { t.Id = default(Int32) }
                var checkRowNull = Expression.IfThenElse(Expression.AndAlso(checkIfContainsColumn, Expression.NotEqual(value, Expression.Constant(System.DBNull.Value))), propertyAssign, propertyAssignDefault);

                setExpressions.Add(checkRowNull);
            }

            var checkIfRowIsNotNull = Expression.IfThen(notNullEqualExpression, Expression.Block(setExpressions));
            var body = Expression.Block(new[] { instanceDeclare }, instanceExpression, checkIfRowIsNotNull, instanceDeclare);
            return Expression.Lambda<Func<DataRow, Entity>>(body, rowDeclare).Compile();
        }
    }
}