using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class ChatSession : BaseEntity
{
    public int UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;

    public User User { get; private set; } = null!;
    public ICollection<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();

    private ChatSession()
    {
    }

    public ChatSession(int userId, string? title = null)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        UserId = userId;
        Title = string.IsNullOrWhiteSpace(title)
            ? "Cuộc trò chuyện mới"
            : title.Trim();
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Session title cannot be empty.", nameof(title));

        Title = title.Trim();
        SetUpdatedAt();
    }

    public void MarkActivity()
    {
        SetUpdatedAt();
    }
}
