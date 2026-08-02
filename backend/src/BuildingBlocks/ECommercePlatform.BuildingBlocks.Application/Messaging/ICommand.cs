using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.BuildingBlocks.Application.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
