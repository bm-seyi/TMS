using MediatR;
using TMS.Domain.Secrets;

namespace TMS.Application.Queries
{
    public sealed record GetArcgisSecretQuery : IRequest<ArcgisSecret>;
}