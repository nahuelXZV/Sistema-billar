using MediatR;

namespace Application.Interfaces;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
