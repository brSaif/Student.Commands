using Grpc.Core;
using gRPCOnHttp3.Data;
using gRPCOnHttp3.Extensions;
using MediatR;
using StudentCommands.EventHistory;

namespace gRPCOnHttp3.Services;

public class EventHistoryService : EventHistory.EventHistoryBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EventHistoryService> _logger;

    public EventHistoryService(
        IMediator mediator,
        ILogger<EventHistoryService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    public override async Task<Response> GetEvents(GetEventsRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Received request to return '{request.PageSize}' events starting from page '{request.CurrentPage}'");
        var req = new GetEvents.GetEventsRequest(request.CurrentPage, request.PageSize);
        var result = await _mediator.Send(req);
        _logger.LogInformation($"The '{result.Count}' Requested events successfully sent");
        return result.ToGetEventsResponse();
    }
}