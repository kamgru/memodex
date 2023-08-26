using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Memodex.WebApp.Common;

public enum NotificationType
{
    Success,
    Error
}

public class Notification
{
    public NotificationType Type { get; init; }
    public required string Message { get; init; }

    public string Serialize() =>
        JsonSerializer.Serialize(this);
    
    public static bool TryDeserialize(
        string? json,
        [NotNullWhen(true)]
        out Notification? notification)
    {
        if (json is null)
        {
            notification = null;
            return false;
        }

        try
        {
            notification = JsonSerializer.Deserialize<Notification>(json);
            return notification is not null;
        }
        catch (JsonException)
        {
            notification = null;
            return false;
        }
    }
}