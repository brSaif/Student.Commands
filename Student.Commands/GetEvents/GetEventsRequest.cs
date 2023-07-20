using gRPCOnHttp3.Domain.Common;
using MediatR;

namespace gRPCOnHttp3.GetEvents;

public record GetEventsRequest(int page = 1, int size = 10) : IRequest<List<Event>>;

// public static class 
