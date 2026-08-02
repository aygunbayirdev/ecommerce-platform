using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.BuildingBlocks.Application.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
