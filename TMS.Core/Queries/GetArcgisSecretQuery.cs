using MediatR;
using TMS.Models.Secrets;

namespace TMS.Core.Queries
{
    public sealed record GetArcgisSecretQuery : IRequest<ArcgisSecret>;
}