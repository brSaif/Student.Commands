using FluentValidation;
using gPPCOnHttp3Server;
using MediatR;
using Student = gRPCOnHttp3.Domain.Student;

namespace gRPCOnHttp3.CreateStudent;

/// <summary>
/// Represent the MediatR create request
/// </summary>
/// <param name="Name">The first name.</param>
/// <param name="Email">The student email.</param>
public record CreateStudentRequest(string Name, string Email, string PhoneNumber) : IRequest<Student>;

public class CreateStudentRequestValidator : AbstractValidator<CreateRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(4)
            .NotEmpty()
            .NotNull()
            .WithMessage("The name must have a minimum size of 4 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .NotNull()
            .EmailAddress()
            .WithMessage("A valid email should have 'name@doamin.whatever'");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .NotNull()
            .Must(x => x.Length == 10)
            .WithMessage("The phone number must be of size 10");
    }
}

