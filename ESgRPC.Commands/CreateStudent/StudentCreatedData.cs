using gRPCOnHttp3.Domain;
using gRPCOnHttp3.Domain.Common;

namespace gRPCOnHttp3.CreateStudent;

public record StudentCreatedData : IEventData
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }

    private StudentCreatedData() 
    {}

    public StudentCreatedData(string Name, string Email, string PhoneNumber)
    {
        this.Name = Name;
        this.Email = Email;
        this.PhoneNumber = PhoneNumber;
    }

    public EventType Type => EventType.StudentCreated;
}