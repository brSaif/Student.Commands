namespace gRPCOnHttp3.Domain.Common;

/// <summary>
/// Represents a generic aggregate root class. 
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class Aggregate<T>
{
    private readonly List<Event> _uncommittedEvents = new();

    /// <summary>
    /// Get and Sets the aggregate Id.
    /// </summary>
    public Guid Id { get; protected set; }
    
    /// <summary>
    /// Get and Sets the sequence Id.
    /// </summary>
    public int Sequence { get; internal set; }
    
    /// <summary>
    /// Get the aggregate events.
    /// </summary>
    public IReadOnlyList<Event> GetUncommittedEvents() => _uncommittedEvents;
    
    /// <summary>
    /// Clear processed aggregate events.
    /// </summary>
    public void MarkChangesAsCommitted() => _uncommittedEvents.Clear();

    /// <summary>
    /// Build the state of an aggregate out ot its list of events.
    /// </summary>
    /// <param name="history">The list of fetched events.</param>
    /// <returns>An aggregate root instance, inheriting from <see cref="Aggregate{T}"/> class.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If an empty event list specified</exception>
    public static T LoadHistoryFromEvents(List<Event> history)
    {
        if (history.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(history), "history.Count == 0");

        var aggregate = (T)Activator.CreateInstance(typeof(T), nonPublic: true);

        foreach (var e in history)
        {
            ((dynamic)aggregate).ApplyChange(e, false);
        }

        return aggregate;
    }

    /// <summary>
    /// Apply the changes received in the event to the aggregate state. 
    /// </summary>
    /// <param name="event">The event to a</param>
    /// <param name="isNew">Sets whether the event is new or not.</param>
    /// <exception cref="InvalidOperationException">If aggregate <see cref="Id"/> is invalid or the <see cref="Sequence"/> doesn't match.</exception>
    protected void ApplyChange(dynamic @event, bool isNew = true)
    {
        if (@event.Sequence == 1)
        {
            Id = @event.AggregateId;
        }

        Sequence++;

        if (Id == Guid.Empty)
            throw new InvalidOperationException("Id == Guid.Empty");

        if (@event.Sequence != Sequence)
            throw new InvalidOperationException("@event.Sequence != Sequence");

        ((dynamic)this).Apply(@event);

        if (isNew)
            _uncommittedEvents.Add(@event);
    }
}