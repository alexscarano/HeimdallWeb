using HeimdallWeb.Application.Common.Interfaces;

namespace HeimdallWeb.Application.Notifications.Commands;

public record ClearAllNotificationsCommand(int UserId);
