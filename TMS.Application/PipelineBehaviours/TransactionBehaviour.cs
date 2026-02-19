using Microsoft.Extensions.Logging;
using System.Diagnostics;
using MediatR;
using TMS.Application.Interfaces;
using TMS.Application.Interfaces.Infrastructure.Persistence;


namespace TMS.Application.PipelineBehaviours
{
    internal sealed class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull, IRequiresTransaction
    {
        private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;
        private readonly ISqlSession _sqlSession;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Application.Behaviours.TransactionBehaviour");

        public TransactionBehaviour(ILogger<TransactionBehaviour<TRequest, TResponse>> logger, ISqlSession sqlSession)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sqlSession = sqlSession ?? throw new ArgumentNullException(nameof(sqlSession));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            using Activity? activity = _activitySource.StartActivity("TransactionBehaviour.Handle");

            string requestName = typeof(TRequest).Name;
            _logger.LogInformation("Starting transaction for {RequestType}", requestName);


            await _sqlSession.BeginAsync(cancellationToken);

            try
            {
                TResponse response = await next(cancellationToken);
                await _sqlSession.CommitAsync(cancellationToken);

                _logger.LogInformation("Transaction committed for {RequestType}", requestName);

                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Transaction failed for {RequestType}. Rolling back...", requestName);
                await _sqlSession.RollbackAsync(cancellationToken);
                _logger.LogInformation("Transaction rolled back for {RequestType}", requestName);
                throw;
            }
        }
    }
}