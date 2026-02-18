using MediatR;
using TMS.Domain.Secrets;

namespace TMS.Core.Queries
{
    public sealed record GetArcgisSecretQuery : IRequest<ArcgisSecret>;
}