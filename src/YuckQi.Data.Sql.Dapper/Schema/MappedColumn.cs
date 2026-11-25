using System.Reflection;

namespace YuckQi.Data.Sql.Dapper.Schema;

internal sealed record MappedColumn(PropertyInfo Property, String Column);
