using Microsoft.EntityFrameworkCore;
using YuckQi.Data.Handlers.Write.Abstract;
using YuckQi.Data.Handlers.Write.Options;
using YuckQi.Domain.Aspects.Abstract.Interfaces;
using YuckQi.Domain.Entities.Abstract.Interfaces;
using YuckQi.Extensions.Mapping.Abstractions.Abstract.Interfaces;

namespace YuckQi.Data.Sql.EntityFramework.Handlers.Write;

public class CreationHandler<TDomainEntity, TIdentifier, TScope>(CreationOptions<TIdentifier>? options = null) : CreationHandler<TDomainEntity, TIdentifier, TScope?, TDomainEntity>(options, null) where TDomainEntity : class, IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier> where TScope : DbContext?;

public class CreationHandler<TDomainEntity, TIdentifier, TScope, TRecord>(CreationOptions<TIdentifier>? options, IMapper? mapper = null) : CreationHandlerBase<TDomainEntity, TIdentifier, TScope?, TRecord>(options, mapper) where TDomainEntity : IDomainEntity<TIdentifier>, ICreationMoment where TIdentifier : IEquatable<TIdentifier> where TScope : DbContext? where TRecord : class
{
    protected override TIdentifier? DoCreate(TDomainEntity entity, TScope? scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = MapToData(entity) ?? throw new InvalidOperationException();

        scope.Set<TRecord>().Add(record);

        return GetIdentifier(scope, record);
    }

    protected override async Task<TIdentifier?> DoCreate(TDomainEntity entity, TScope? scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var record = MapToData(entity) ?? throw new InvalidOperationException();

        await scope.Set<TRecord>().AddAsync(record, cancellationToken);

        return GetIdentifier(scope, record);
    }

    private static TIdentifier? GetIdentifier(DbContext context, TRecord record)
    {
        var entry = context.Entry(record);
        var key = entry.Metadata.FindPrimaryKey();

        if (key?.Properties.FirstOrDefault()?.Name is not { } keyPropertyName)
            return default;

        var value = entry.Property(keyPropertyName).CurrentValue;

        return value is not null ? (TIdentifier) value : default;
    }
}
