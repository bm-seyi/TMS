using MediatR;

namespace TMS.Application.Events
{
    public sealed record LinesUpdatedNotification(string Update) : INotification;
}