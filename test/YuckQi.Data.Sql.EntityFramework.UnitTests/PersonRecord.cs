using YuckQi.Domain.Entities.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests;

internal class PersonRecord : IDomainEntity<Int32>
{
    public PersonAddress Address { get; set; } = new();

    public String FirstName { get; set; } = String.Empty;

    public Int32 Id { get; set; }

    public Int32 Identifier { get => Id; set => Id = value; }

    public String LastName { get; set; } = String.Empty;
}
