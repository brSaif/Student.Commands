using ES_gRPC.UnitTests;
using Grpc.Net.Client;
using gRPCOnHttp3;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = TestBase.UseSqlDataBase)]
namespace ES_gRPC.UnitTests;

public abstract class TestBase
    {
        public const bool UseSqlDataBase = false;
        private GrpcChannel _channel;
        private TestWebApplicationFactory<Program> _factory;

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }

        public GrpcChannel Channel
        {
            get
            {
                if (_channel != null)
                    return _channel;

                Initialize();
                return _channel ?? throw new Exception("return _channel");
            }

            private set => _channel = value;
        }
        protected TestWebApplicationFactory<Program> Factory
        {
            get
            {
                if (_factory != null)
                    return _factory;

                Initialize();
                return _factory ?? throw new Exception("return _factory");
            }

            private set => _factory = value;
        }

        public void Initialize(
            Action<IServiceCollection> configureTopic = null,
            Action<IServiceCollection> configureOther = null
        )
        {
            if (configureTopic == null)
                configureTopic = services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IServiceBusPublisher));

                    services.Remove(descriptor);

                    var mock = new Mock<IServiceBusPublisher>();

                    // todo : check this later
                    var messagesMock = new Mock<IEnumerable<OutboxMessage>>(); 
                    mock.Setup(t => t.PublishAsync(messagesMock.Object));

                    services.AddSingleton(mock.Object);
                };

            void Configure(IServiceCollection services)
            {
                configureTopic?.Invoke(services);
                configureOther?.Invoke(services);
            };

            var factory = new TestWebApplicationFactory<Program>(Output, Configure);

            var client = factory.CreateClient();

            Channel = GrpcChannel.ForAddress(client.BaseAddress ?? throw new Exception(), new GrpcChannelOptions()
            {
                HttpClient = client,
            });

            Factory = factory;

            ResetDb();
        }

        private void ResetDb()
        {
#pragma warning disable CS8520 // The given expression always matches the provided constant.
            if (UseSqlDataBase is false)
                return;

            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
            context.EventStore.RemoveRange(context.EventStore);
            context.UniqueReferences.RemoveRange(context.UniqueReferences);
            context.OutboxMessages.RemoveRange(context.OutboxMessages);
            context.SaveChanges();
#pragma warning restore CS8520 // The given expression always matches the provided constant.
        }

    }