using gPPCOnHttp3Server;
using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.Domain.Common;
using gRPCOnHttp3.Extensions;
using gRPCOnHttp3.UpdateStudent;
using UpdateStudentRequest = gRPCOnHttp3.UpdateStudent.UpdateStudentRequest;

namespace gRPCOnHttp3.Domain;

/// <summary>
/// Represents the <see cref="Student"/> entity.
/// </summary>
public sealed class Student : Aggregate<Student>
{
    private Student (){}
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }

    /// <summary>
    /// Initializes a new instance of <see cref="Student"/> class.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="email">The student email.</param>
    /// <exception cref="ArgumentException"></exception>
    private Student(string name, string email)
    {
        Ensure.NullOrWhiteSpace(name);
        Ensure.NullOrWhiteSpace(email);
        
        Name = name;
        Email = email;
    }

    public static Student Create(CreateStudentRequest request)
    {
        var @event = request.ToEvent();
        Student student = new();
        student.ApplyChange(@event);
        return student;
    }

    /// <summary>
    /// Set the <see cref="Student"/> email.
    /// </summary>
    /// <param name="newEmail">The new email input</param>
    /// <exception cref="ArgumentException">If the new email validation fails</exception>
    public void Update(UpdateStudentRequest request)
    {
        var @event = request.ToEvent(Sequence + 1);
        ApplyChange(@event);
    }


    public void Apply(StudentCreatedEvent @event)
    {
        this.Name = @event.Data.Name;
        this.Email = @event.Data.Email;
        this.PhoneNumber = @event.Data.PhoneNumber;
    }

    public void Apply(StudentUpdatedEvent updatedEvent)
    {
        this.Name = updatedEvent.Data.Name;
        this.Email = updatedEvent.Data.Email;
        this.PhoneNumber = updatedEvent.Data.PhoneNumber;
    }
}