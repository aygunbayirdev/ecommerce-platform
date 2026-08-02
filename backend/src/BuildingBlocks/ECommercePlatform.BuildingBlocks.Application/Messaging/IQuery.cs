using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.BuildingBlocks.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
