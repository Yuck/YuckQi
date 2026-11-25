using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests;

internal class ManualKeyRecord : IDomainEntity<Int32>, ICreationMoment
{
    public DateTimeOffset CreationMoment { get; set; }

    public Int32 Id { get; set; }

    public Int32 Identifier { get => Id; set => Id = value; }

    public String Name { get; set; } = String.Empty;
}
