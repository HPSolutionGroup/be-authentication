using HP.Authentication.Application.Abstractions.Repository;
using MediatR;

namespace HP.Authentication.Application.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionBehavior(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var isCommand = typeof(TRequest).Name.EndsWith("Command");

            if (!isCommand)
                return await next();

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var response = await next();

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return response;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}
