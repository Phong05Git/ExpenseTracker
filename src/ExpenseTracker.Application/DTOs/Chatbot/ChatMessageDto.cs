namespace ExpenseTracker.Application.DTOs.Chatbot;

public class ChatMessageDto
{
    public int Id { get; init; }
    public int ChatSessionId { get; init; }
    public string Role { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
