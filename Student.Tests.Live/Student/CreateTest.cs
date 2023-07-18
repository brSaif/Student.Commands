using ES_gRPC.UnitTests;
using ES_gRPC.UnitTests.Grpc.Protos;
using ESgRPC.Tests.Live.Asserts;
using Google.Protobuf.WellKnownTypes;
using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ESgRPC.Tests.Live.Student;

public class CreateTest : TestBase
{
    public const int Delay = 6_000;
    
    public CreateTest(ITestOutputHelper output) : base(output)
    { }

    [Theory]
    [InlineData("steve", "email@test1.com", "0123456789")]
    [InlineData("steven", "email@test2.com", "0123456789")]
    public async Task Create_WhenGivenValidData_ReturnsTheCreatedStudent(string name, string email, string phone)
    {
        Initialize(configureTopic: s=> {});
        
        var configuration = Factory.Services.GetRequiredService<IConfiguration>();

        var listener = new Listener(configuration);

        var grpcClient = new ES_gRPC.UnitTests.Grpc.Protos.Student.StudentClient(Channel);

        var createRequest = new CreateRequest
        {
            Name = name,
            Email = email,
            PhoneNumber = phone
        };
        
        // Act
        await grpcClient.CreateAsync(createRequest);

        await Task.Delay(Delay);

        await listener.CloseAsync();

        using var scope = Factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var studentCreateEvent = await context.EventStore
            .OfType<StudentCreatedEvent>()
            .SingleOrDefaultAsync();
        
        var outboxMessage = await context.OutboxMessages.SingleOrDefaultAsync();
        var message = listener.Messages.SingleOrDefault();
        
        // Assert
        
        Assert.Null(outboxMessage);
        Assert.Equal(1, await context.EventStore.CountAsync());
        MessageAsserts.AssertEquality(studentCreateEvent, message);
    }
}