using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ES_gRPC.UnitTests;

public static class AppDbContextExtension
{
    public static async Task BuildUniqueRecordsAsync(
        this AppDbContext context,
        ILogger logger = null
    )
    {
        if (await context.UniqueReferences.AnyAsync())
            return;

        var events = await context.EventStore
            .OfType<StudentCreatedEvent>()
            .ToListAsync();

        foreach (var @event in events)
        {
            var data = @event.Data;

            var createCommand = new CreateStudentRequest(
                Name: data.Name,
                Email: data.Email,
                PhoneNumber: data.PhoneNumber
                );

            var student = Student.Create(createCommand);

            var uniqueNameReference = new UniqueReference(student);

            await context.UniqueReferences.AddAsync(uniqueNameReference);
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Build unique tables failed.");
            throw;
        }
    }

}