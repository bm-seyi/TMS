using MediatR;
using TMS.Core.Interfaces;
using TMS.Domain.DTOs;

namespace TMS.Core.Queries
{
    public sealed record LinesDataQuery : IRequest<IEnumerable<LinesReadDTO>>, IRequiresConnection;
}