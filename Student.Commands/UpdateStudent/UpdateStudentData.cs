using System.Runtime.CompilerServices;
using gRPCOnHttp3.Domain.Common;

namespace gRPCOnHttp3.UpdateStudent;

/// <summary>
/// Represents the wrapper for <see cref="UpdateStudentData"/>.  
/// </summary>
/// <param name="email"></param>
public record UpdateStudentData : IEventData
{
    public string Name { get; private set; }
    public string Email { get; private set;}
    public string PhoneNumber { get; private set;}

    public UpdateStudentData(string Name, string Email, string PhoneNumber)
    {
        this.Name = Name;
        this.Email = Email;
        this.PhoneNumber = PhoneNumber;
    }

    private UpdateStudentData()
    { }
    
    /// <summary>
    /// Gets the <see cref="UpdateStudentData"/> event data.
    /// </summary>
    public EventType Type => EventType.StudentUpdated;
}