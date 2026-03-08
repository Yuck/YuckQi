namespace YuckQi.Data.Handlers.Write.Options;

public class RevisionOptions(PropertyHandling revisionMomentAssignment = PropertyHandling.Manual)
{
    public PropertyHandling RevisionMomentAssignment { get; } = revisionMomentAssignment;
}
