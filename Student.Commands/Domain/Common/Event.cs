namespace gRPCOnHttp3.Domain.Common;

public abstract class Event
{
    protected Event()
    {
    }

    public long Id { get; protected set; }
    public Guid AggregateId { get; protected set; }
    public int Sequence { get; protected set; }
    public EventType Type { get; protected set; }
    public DateTime DateTime { get; protected set; }
    public int Version { get; protected set; }
}

public abstract class Event<TData> : Event
    where TData : IEventData
{
    protected Event()
    {
    }

    protected Event(
        Guid aggregateId,
        int sequence,
        TData data,
        int version = 1
    )
    {
        AggregateId = aggregateId;
        Sequence = sequence;
        Type = data.Type;
        Data = data;
        DateTime = DateTime.UtcNow;
        Version = version;
    }

    public TData Data { get; protected set; }
}

public enum EventType
{
    StudentCreated = 0,
    StudentUpdated = 1
}