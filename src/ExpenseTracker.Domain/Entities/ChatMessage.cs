using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public const string RoleUser = "user";
    public const string RoleAssistant = "assistant";

    public int ChatSessionId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;

    public ChatSession ChatSession { get; private set; } = null!;

    private ChatMessage()
    {
    }

    public ChatMessage(int chatSessionId, string role, string content)
    {
        if (chatSessionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(chatSessionId));

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Message role cannot be empty.", nameof(role));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.", nameof(content));

        var normalizedRole = role.Trim().ToLowerInvariant();

        if (normalizedRole is not RoleUser and not RoleAssistant)
            throw new ArgumentOutOfRangeException(
                nameof(role),
                "Role must be either 'user' or 'assistant'.");

        ChatSessionId = chatSessionId;
        Role = normalizedRole;
        Content = content;
    }
}
