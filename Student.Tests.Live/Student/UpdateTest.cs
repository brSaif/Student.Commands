using ES_gRPC.UnitTests;
using ES_gRPC.UnitTests.Fakers;
using ES_gRPC.UnitTests.Grpc.Protos;
using ESgRPC.Tests.Live.Asserts;
using Google.Protobuf.WellKnownTypes;
using gPPCOnHttp3Server;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.UpdateStudent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using UpdateStudentRequest = ES_gRPC.UnitTests.Grpc.Protos.UpdateStudentRequest;

namespace ESgRPC.Tests.Live.Student;

public class UpdateTest : TestBase
{
    private const int Delay = 6_000;

    public UpdateTest(ITestOutputHelper output) : base(output)
    { }

    [Theory]
    [InlineData("Sameer", "sameer@test.com", "0123456789")]
    [InlineData("Salem", "salem@test.com", "0123456789")]
    public async Task Update_WhenGivenValidData_ReturnTheUpdatedStudent(string name, string email, string phone)
    {
        Initialize(configureTopic: s => { });

        var configuration = Factory.Services.GetRequiredService<IConfiguration>();

        var listener = new Listener(configuration);

        var grpcClient = new ES_gRPC.UnitTests.Grpc.Protos.Student.StudentClient(Channel);

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

        //Act
        await grpcClient.UpdateAsync(updateRequest);

        await Task.Delay(Delay);

        await listener.CloseAsync();

        var cardComplaintUpdated = await context.EventStore
            .OfType<StudentUpdatedEvent>()
            .SingleOrDefaultAsync();

        var outboxMessage = await context.OutboxMessages.SingleOrDefaultAsync();

        var message = listener.Messages.SingleOrDefault();

        // Assert
        Assert.Null(outboxMessage);

        MessageAsserts.AssertEquality(cardComplaintUpdated, message);
    }
}