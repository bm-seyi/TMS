using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TMS.Core.Interfaces.Persistence;

namespace TMS.Core.Behaviours
{
    internal sealed class ConnectionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<ConnectionBehaviour<TRequest, TResponse>> _logger;
        private readonly ISqlSession _sqlSession;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Core.Behaviours.ConnectionBehaviour");

        public ConnectionBehaviour(ILogger<ConnectionBehaviour<TRequest, TResponse>> logger, ISqlSession sqlSession)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sqlSession = sqlSession ?? throw new ArgumentNullException(nameof(sqlSession));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            using (Activity? activity = _activitySource.StartActivity("ConnectionBehaviour.Handle"))
            {
                await _sqlSession.OpenAsync(cancellationToken);
                _logger.LogDebug("SQL connection is open for request of type {RequestType}.", typeof(TRequest).Name);
            }

            return await next(cancellationToken);
        }
    }
}