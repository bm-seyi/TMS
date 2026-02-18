using MediatR;
using TMS.Application.Interfaces;
using TMS.Domain.DTOs;

namespace TMS.Application.Queries
{
    public sealed record DatabaseHealthCheckQuery : IRequest<DatabaseHealthCheckDTO>, IRequiresConnection;
}