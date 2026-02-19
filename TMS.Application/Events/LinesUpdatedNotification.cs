using MediatR;
using TMS.Domain.DTOs;

namespace TMS.Application.Events
{
    public sealed record LinesUpdatedNotification(string Update) : INotification;
}