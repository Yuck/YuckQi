namespace YuckQi.Data.Handlers.Write.Options;

public class CreationOptions<TIdentifier>(Func<TIdentifier>? identifierFactory = null, PropertyHandling creationMomentAssignment = PropertyHandling.Manual, PropertyHandling revisionMomentAssignment = PropertyHandling.Manual)
{
    public PropertyHandling CreationMomentAssignment { get; } = creationMomentAssignment;

    public Func<TIdentifier>? IdentifierFactory { get; } = identifierFactory;

    public PropertyHandling RevisionMomentAssignment { get; } = revisionMomentAssignment;
}
