using FluentValidation;
using gRPCOnHttp3.Domain;
using MediatR;

namespace gRPCOnHttp3.UpdateStudent;

/// <summary>
/// Represent the MediatR create request
/// </summary>
/// <param name="firstName">The first name.</param>
/// <param name="lastName">The last name.</param>
/// <param name="email">The student email.</param>
public record UpdateStudentRequest(string StudentId, string Name, string Email, string PhoneNumber) 
    : IRequest<Student>;
    
public class UpdateStudentRequestValidator : AbstractValidator<gPPCOnHttp3Server.UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => Guid.TryParse(id, out var _))
            .WithMessage("Invalid student identifier specified, needs to be a Guid that contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)");
        
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