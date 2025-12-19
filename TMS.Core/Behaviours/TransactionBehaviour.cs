using Microsoft.Extensions.Logging;
using System.Diagnostics;
using MediatR;
using TMS.Core.Interfaces.Persistence;
using TMS.Core.Interfaces;


namespace TMS.Core.Behaviours
{
    internal sealed class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;
        private readonly ISqlSession _sqlSession;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Core.Behaviours.TransactionBehaviour");

        public TransactionBehaviour(ILogger<TransactionBehaviour<TRequest, TResponse>> logger, ISqlSession sqlSession)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sqlSession = sqlSession ?? throw new ArgumentNullException(nameof(sqlSession));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {

            if (request is IReadOnlyRequest)
            {
                _logger.LogDebug("Request of type {RequestType} does not require a transaction.", typeof(TRequest).Name);
                return await next(cancellationToken);
            }

            using Activity? activity = _activitySource.StartActivity("TransactionBehaviour.Handle");

            await _sqlSession.BeginAsync(cancellationToken);

            try
            {
                TResponse response = await next(cancellationToken);
                await _sqlSession.CommitAsync(cancellationToken);
                return response;
            }
            catch
            {
                await _sqlSession.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}