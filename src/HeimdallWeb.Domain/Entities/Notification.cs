using HeimdallWeb.Domain.Enums;

namespace HeimdallWeb.Domain.Entities;

public class Notification
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core
    private Notification() { }

    public Notification(int userId, string title, string body, NotificationType type)
    {
        UserId = userId;
        Title = title;
        Body = body;
        Type = type;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead() => IsRead = true;
}
