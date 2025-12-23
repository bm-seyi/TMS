using MediatR;
using TMS.Core.Interfaces;
using TMS.Models.DTOs;

namespace TMS.Core.Queries
{
    public sealed record DatabaseHealthCheckQuery : IRequest<DatabaseHealthCheckDTO>, IRequiresConnection;
}