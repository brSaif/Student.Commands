using System.Reflection;
using Azure.Messaging.ServiceBus;
using Calzolari.Grpc.AspNetCore.Validation;
using FluentValidation;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Extensions;
using gRPCOnHttp3.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentCommands.EventHistory;

var builder = WebApplication.CreateBuilder(args);


// set up the kestrel host.
builder.WebHost.ConfigureKestrel((ctx, opt) =>
{
    opt.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
        listenOptions.UseHttps();
    });
});

builder.Services.AddGrpc();

builder.Configuration.AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly());

var connectionString = builder.Configuration.GetConnectionString("MssqlDatabase");

Console.WriteLine($"Connection String: {connectionString ?? "failed to fetch the connection string"}");

builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseSqlServer(connectionString));

builder.Services.AddMediatR(c
    => c.RegisterServicesFromAssemblyContaining<gRPCOnHttp3.Program>());

builder.Services.Configure<ServiceBusPublisherOptions>(
    opt => builder.Configuration.GetSection("ServiceBus").Bind(opt));

builder.Services.AddSingleton(
    sp =>
    {
        var opt = sp.GetRequiredService<IOptions<ServiceBusPublisherOptions>>();
        return new ServiceBusClient(opt.Value.ConnectionString);
    });
builder.Services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();


// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddGrpcFluentValidationValidators();
// builder.Services.AddValidatorsFromAssemblyContaining<gRPCOnHttp3.Program>();

var app = builder.Build();

app.MapGrpcService<StudentService>();
app.MapGrpcService<gRPCOnHttp3.Services.DemoEvents>();
app.MapGrpcService<EventHistoryService>();

app.MapGet("/", () => "Hello World!");

app.Run();


namespace gRPCOnHttp3
{
    public partial class Program {}
}