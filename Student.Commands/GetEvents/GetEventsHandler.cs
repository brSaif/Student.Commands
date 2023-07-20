using gRPCOnHttp3.Data;
using gRPCOnHttp3.Domain.Common;
using gRPCOnHttp3.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace gRPCOnHttp3.GetEvents;

public class GetEventsHandler : IRequestHandler<GetEventsRequest, List<Event>>
{
    private readonly AppDbContext _context;

    public GetEventsHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Event>> Handle(GetEventsRequest request, CancellationToken cancellationToken)
        => await _context
            .EventStore
            .AsNoTracking()
            .OrderBy(e => e.AggregateId)
            .ThenBy(e => e.Sequence)
            .Skip(request.size * (request.page - 1))
            .Take(request.size)
            .ToListAsync(cancellationToken);
}