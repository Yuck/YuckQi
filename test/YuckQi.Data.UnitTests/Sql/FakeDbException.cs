using System.Data.Common;

namespace YuckQi.Data.UnitTests.Sql;

public sealed class FakeDbException : DbException
{
    public FakeDbException(String message) : base(message)
    {
    }
}
