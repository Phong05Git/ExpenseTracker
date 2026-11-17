namespace ExpenseTracker.Application.DTOs.Chatbot;

public class ChatSessionDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? LastMessagePreview { get; init; }
}
