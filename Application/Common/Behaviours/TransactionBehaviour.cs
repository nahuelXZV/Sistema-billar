using Application.Interfaces;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Common.Behaviours;

public class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ICommand<TResponse>
{
    private readonly IDbContext _appCnx;

    public TransactionBehaviour(IDbContext appCnx)
    {
        _appCnx = appCnx;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_appCnx.HasActiveTransaction)
        {
            return await next();
        }

        using var transaction = await _appCnx.BeginTransactionAsync();

        try
        {
            var response = await next();
            await _appCnx.CommitTransactionAsync(transaction);
            return response;
        }
        catch
        {
            if (_appCnx.HasActiveTransaction)
            {
                await _appCnx.RollbackTransactionAsync();
            }

            throw;
        }
    }
}
