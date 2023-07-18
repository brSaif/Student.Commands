using ES_gRPC.UnitTests.Asserts;
using ES_gRPC.UnitTests.Fakers;
using ES_gRPC.UnitTests.Grpc.Protos;
using Grpc.Core;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ES_gRPC.UnitTests.Students;

public class UpdateStudentTests : TestBase
{
    public const string InvalidStudentAggregateIdErrorMessage =
        "Invalid student identifier specified, needs to be a Guid that contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)";
    public const string InvalidNameErrorMessage = "The name must have a minimum size of 4 characters";
    public const string InvalidEmailErrorMessage = "A valid email should have 'name@doamin.whatever'";
    public const string InvalidPhoneNumberErrorMessage = "The phone number must be of size 10";
    
    public const string NameIsNotUpdatedErrorMessage = "You need to update the name too.";
    public const string NoStudentAggregateWasFoundThatMatchesTheGivenStudentId = "No student aggregate with the Id:";
    
    public UpdateStudentTests(ITestOutputHelper output) : base(output)
    { }

    [Theory] 
    [InlineData("BE3DA377-5037", "valid name", "email@test.com", "0123654789", new string[]{InvalidStudentAggregateIdErrorMessage})]
    [InlineData("BE3DA377-5037-474C-92C0-F34583557A4A", "", "test@test.com", "0123654789", new string[]{InvalidNameErrorMessage})]
    [InlineData("BE3DA377-5037-474C-92C0-F34583557A4A", "valid name", "test.com", "0123654789", new string[]{InvalidEmailErrorMessage})]
    [InlineData("BE3DA377-5037-474C-92C0-F34583557A4A", "valid name", "email@test.com", "0123456", new string[]{InvalidPhoneNumberErrorMessage})]
    [InlineData("BE3DA377-5037", "vme", "est.com", "0654789",new string[]{InvalidNameErrorMessage, InvalidEmailErrorMessage, InvalidPhoneNumberErrorMessage, InvalidStudentAggregateIdErrorMessage})]
    public async Task Update_WhenGivenInvalidDate_ThrowsAnRPCValidationException(
        string studentId, string name, string email, string phone, string[] expectedErrorMessges)
    {
        var grpcClient = new Student.StudentClient(Channel);

        var updateRequest = new UpdateStudentRequest()
        {
            Id = studentId,
            Name = name,
            Email = email ,
            PhoneNumber = phone
        };

        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await grpcClient.UpdateAsync(updateRequest)
        );
        
        Assert.NotEmpty(exception.Message);
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }
    
    [Theory]    
    [InlineData("BE3DA377-5037-474C-92C0-F34583557A4A", "valid name", "email@test.com", "0123654789", NameIsNotUpdatedErrorMessage)]
    public async Task Update_WhenTheNameInTheRequestIsAlreadyTakenByAnotherUser_ThrowsANameAlreadyExistRPCException(
        string studentId, string name, string email, string phone, string expectedErrorMessges)
    {
        var grpcClient = new Student.StudentClient(Channel);

        using var scope = Factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var aggregateId = Guid.Parse(studentId);
        var createdEvent = new StudentCreatedFaker()
            .RuleFor(
                c => c.Data, 
                new StudentCreatedDataFaker()
                    .RuleFor(x => x.Name, name)
                    .RuleFor(x => x.PhoneNumber, phone)
                    .Generate()
            )
            .Generate();
        
        var createdEvent2 = new StudentCreatedFaker()
            .RuleFor(
                c => c.Data, 
                new StudentCreatedDataFaker()
                    .RuleFor(x => x.PhoneNumber, phone)
                    .Generate()
            )
            .RuleFor(x => x.AggregateId, aggregateId)
            .Generate();

        await context.EventStore.AddRangeAsync(new []{createdEvent, createdEvent2});
        await context.SaveChangesAsync();
        await context.BuildUniqueRecordsAsync();
        context.ChangeTracker.Clear();
        
        var updateStudentRequest = new UpdateStudentRequest()
        {
            Id = aggregateId.ToString(),
            Name = name,
            Email = email,
            PhoneNumber = phone,
        };
        
        // Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await grpcClient.UpdateAsync(updateStudentRequest)
        );
        
        Assert.NotEmpty(exception.Message);
        Assert.Equal(StatusCode.AlreadyExists, exception.StatusCode);

    }
    
    [Theory]    
    [InlineData("BE3DA377-5037-474C-92C0-F34583557A4A", "valid name", "email@test.com", "0123654789", NoStudentAggregateWasFoundThatMatchesTheGivenStudentId)]
    public async Task Update_WhenGivenStudentIdNonExistentInDB_ThrowsNoStudentWasFoundRPCException(
        string nonExistantStudentId, string name, string email, string phone, string expectedErrorMessges)
    {
        var grpcClient = new Student.StudentClient(Channel);

        var updateStudentRequest = new UpdateStudentRequest()
        {
            Id = nonExistantStudentId,
            Name = name,
            Email = email,
            PhoneNumber = phone,
        };

        
        // Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await grpcClient.UpdateAsync(updateStudentRequest)
        );
        
        Assert.NotEmpty(exception.Message);
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Theory]
    [InlineData("Sameer", "sameer@test.com", "0123456789")]
    [InlineData("Salem", "salem@test.com", "0123456789")]
    public async Task Update_WhenGivenValidData_ReturnTheUpdatedStudent(string name, string email, string phone)
    {
        
        var grpcClient = new Student.StudentClient(Channel);

        using var scope = Factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var aggregateId = Guid.NewGuid();
        var createdEvent = new StudentCreatedFaker()
            .RuleFor(
                c => c.Data, 
                new StudentCreatedDataFaker()
                    .RuleFor(x => x.PhoneNumber, "0123456789")
                    .Generate()
            )
            .RuleFor(x => x.AggregateId, aggregateId)
            .Generate();

        await context.EventStore.AddAsync(createdEvent);

        await context.SaveChangesAsync();

        await context.BuildUniqueRecordsAsync();

        context.ChangeTracker.Clear();

        var updateRequest = new UpdateStudentRequest()
        {
            Id = createdEvent.AggregateId.ToString(),
            Email = email,
            PhoneNumber = phone,
            Name = name,
        };
       

        var response = await grpcClient.UpdateAsync(updateRequest);

        var studentUpdatedEvent = await context.EventStore
            .SingleOrDefaultAsync(e => e.Type == EventType.StudentUpdated);

        var message = await context.OutboxMessages.SingleOrDefaultAsync();

        var uniqueReferences = await context.UniqueReferences.SingleOrDefaultAsync();

        // Assert
        EventAssert.AssertEquality(
            updateStudentRequest: updateRequest,
            studentResponse: response,
            studentEvent: studentUpdatedEvent,
            sequence: 2
        );

        EventAssert.AssertEquality(
            message: message,
            updatedStudentEvent: studentUpdatedEvent);

        Assert.NotNull(uniqueReferences);
        Assert.Equal(uniqueReferences.Name, createdEvent.Data.Name);
        Assert.NotEmpty(response.PhoneNumber);
    }

}