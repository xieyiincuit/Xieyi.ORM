using Xieyi.ORM.Core.Exceptions;

namespace Xieyi.ORM.Core.DbContext;

public enum DataBaseType
{
    SqlServer,
    MySql,
    MongoDB
}

internal enum DataBaseCategory
{
    SQL,
    NoSQL
}

internal static class DataBaseCategoryFilter
{
    internal static DataBaseCategory GetCategory(this DataBaseType dataBaseType)
    {
        switch (dataBaseType)
        {
            case DataBaseType.SqlServer:
                return DataBaseCategory.SQL;
            case DataBaseType.MySql:
                return DataBaseCategory.SQL;
            case DataBaseType.MongoDB:
                return DataBaseCategory.NoSQL;
            default:
                break;
        }

        throw new UnknownDataBaseTypeException("You DataBaseType Is UnSupported");
    }
}