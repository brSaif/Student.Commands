using ES_gRPC.UnitTests.Asserts;
using ES_gRPC.UnitTests.Fakers;
using ES_gRPC.UnitTests.Grpc.Protos;
using Grpc.Core;
using gRPCOnHttp3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace ES_gRPC.UnitTests.Students;

public class CreateStudent : TestBase
{
    public const string InvalidNameErrorMessage = "The name must have a minimum size of 4 characters";
    public const string InvalidEmailErrorMessage = "A valid email should have 'name@doamin.whatever'";
    public const string InvalidPhoneNumberErrorMessage = "The phone number must be of size 10";
    public const string ValidNameButTakenInDbErrorMessage = "Already exists";



    public CreateStudent(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }
    
    [Theory]
    [InlineData("", "test@test.com", "0123654789", new string[]{InvalidNameErrorMessage})]
    [InlineData("valid name", "test.com", "0123654789", new string[]{InvalidEmailErrorMessage})]
    [InlineData("valid name", "email@test.com", "0123456", new string[]{InvalidPhoneNumberErrorMessage})]
    [InlineData("vme", "est.com", "0654789",new string[]{InvalidNameErrorMessage, InvalidEmailErrorMessage, InvalidPhoneNumberErrorMessage})]
    public async Task Create_WhenGivenInvalidData_ThrowsAValidationException(
        string name, string email, string phoneNumber, string[] errors
        )
    {
        
        var grpcClient = new Student.StudentClient(Channel);

        var createRequest = new CreateRequest()
        {
            Name = name,
            Email = email ,
            PhoneNumber = phoneNumber
        };

        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await grpcClient.CreateAsync(createRequest)
        );
        
        Assert.NotEmpty(exception.Message);
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
        // Assert.Contains(exception.Get...(), errors);
    }
    
    [Fact]
    public async Task Create_WhenGivenANameAlreadyTaken_ThrowsAlreadyExistsException()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createdEvent = new StudentCreatedFaker()
            .RuleFor(
                s => s.Data, 
                new StudentCreatedDataFaker()
                    .RuleFor(x => x.PhoneNumber, "0123456798")
                    .Generate()
                )
            .Generate();

        await context.EventStore.AddAsync(createdEvent);
        
        await context.SaveChangesAsync();

        await context.BuildUniqueRecordsAsync();

        var grpcClient = new Student.StudentClient(Channel);

        var createRequest = new CreateRequest()
        {
            Name = createdEvent.Data.Name,
            Email = createdEvent.Data.Email,
            PhoneNumber = createdEvent.Data.PhoneNumber
        };

        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await grpcClient.CreateAsync(createRequest)
            );
        
        Assert.NotEmpty(exception.Message);
        Assert.Equal(StatusCode.AlreadyExists, exception.StatusCode);
    }
    
    [Theory]
    [InlineData("steve", "steve@email.com", "0923232323")]
    [InlineData("steven", "steven@email.com", "0975643532")]
    public async Task Create_WhenGivenValidData_ReturnsTheCreatedStudent(string name, string email, string phone)
    {
        // Arrange
        var grpcClient = new Student.StudentClient(Channel);
        CreateRequest createRequest = new()
        {
            Name = name,
            Email = email,
            PhoneNumber = phone
        };
        
        // Act
        var response = await grpcClient.CreateAsync(createRequest);
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var StudentEvent = await context.EventStore.SingleOrDefaultAsync();
        var uniqueNameReferences = await context.UniqueReferences.SingleOrDefaultAsync();
        var message = await context.OutboxMessages.SingleOrDefaultAsync();
        
        //Assert
        EventAssert.AssertEquality(
            createRequest: createRequest,
            studentResponse: response,
            studentEvent: StudentEvent);
     
        Assert.Equal(response.Id, uniqueNameReferences.Id.ToString());
        Assert.Equal(response.Name, uniqueNameReferences.Name);
        
        EventAssert.AssertEquality(StudentEvent, message);
    }
}