using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Chatbot;
using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class ChatService(
    IChatSessionRepository chatSessionRepository,
    IChatMessageRepository chatMessageRepository,
    IUnitOfWork unitOfWork) : IChatService
{
    private const int MaxPageSize = 50;
    private const int MaxTitleLength = 120;
    private const int MaxMessageLength = 8000;
    private const int PreviewLength = 120;

    public async Task<Result<ChatSessionDto>> CreateSessionAsync(
        int userId,
        CreateChatSessionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return Result<ChatSessionDto>.Failure("Authenticated user is required.");

        var title = request.Title?.Trim();
        if (title?.Length > MaxTitleLength)
        {
            return Result<ChatSessionDto>.Failure(
                $"Session title cannot exceed {MaxTitleLength} characters.");
        }

        var session = new ChatSession(userId, title);

        await chatSessionRepository.AddAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ChatSessionDto>.Success(MapSession(session, null));
    }

    public async Task<Result<PagedResultDto<ChatSessionDto>>> GetSessionsAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return Result<PagedResultDto<ChatSessionDto>>.Failure(
                "Authenticated user is required.");

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var sessions = await chatSessionRepository.GetByUserIdAsync(
            userId,
            page,
            pageSize,
            cancellationToken);

        var totalItems = await chatSessionRepository.CountByUserIdAsync(
            userId,
            cancellationToken);

        var items = sessions
            .Select(session => MapSession(
                session,
                session.Messages
                    .OrderByDescending(message => message.CreatedAt)
                    .ThenByDescending(message => message.Id)
                    .FirstOrDefault()?.Content))
            .ToList();

        return Result<PagedResultDto<ChatSessionDto>>.Success(
            new PagedResultDto<ChatSessionDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            });
    }

    public async Task<Result<PagedResultDto<ChatMessageDto>>> GetMessagesAsync(
        int userId,
        int sessionId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return Result<PagedResultDto<ChatMessageDto>>.Failure(
                "Authenticated user is required.");

        if (sessionId <= 0)
            return Result<PagedResultDto<ChatMessageDto>>.Failure(
                "Chat session not found.");

        var session = await chatSessionRepository.GetOwnedByIdAsync(
            sessionId,
            userId,
            cancellationToken);

        if (session == null)
        {
            // Do not reveal whether a session belonging to another user exists.
            return Result<PagedResultDto<ChatMessageDto>>.Failure(
                "Chat session not found.");
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var messages = await chatMessageRepository.GetBySessionIdAsync(
            sessionId,
            page,
            pageSize,
            cancellationToken);

        var totalItems = await chatMessageRepository.CountBySessionIdAsync(
            sessionId,
            cancellationToken);

        return Result<PagedResultDto<ChatMessageDto>>.Success(
            new PagedResultDto<ChatMessageDto>
            {
                Items = messages.Select(MapMessage).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            });
    }

    public async Task<Result<ChatMessageDto>> AddUserMessageAsync(
        int userId,
        int sessionId,
        SendChatMessageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return Result<ChatMessageDto>.Failure("Authenticated user is required.");

        if (sessionId <= 0)
            return Result<ChatMessageDto>.Failure("Chat session not found.");

        var content = request.Content?.Trim();
        if (string.IsNullOrWhiteSpace(content))
            return Result<ChatMessageDto>.Failure("Message content cannot be empty.");

        if (content.Length > MaxMessageLength)
        {
            return Result<ChatMessageDto>.Failure(
                $"Message content cannot exceed {MaxMessageLength} characters.");
        }

        var session = await chatSessionRepository.GetOwnedByIdAsync(
            sessionId,
            userId,
            cancellationToken);

        if (session == null)
        {
            // Ownership is checked on the server; the client never supplies UserId.
            return Result<ChatMessageDto>.Failure("Chat session not found.");
        }

        var message = new ChatMessage(
            sessionId,
            ChatMessage.RoleUser,
            content);

        session.MarkActivity();
        await chatMessageRepository.AddAsync(message, cancellationToken);
        chatSessionRepository.Update(session);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ChatMessageDto>.Success(MapMessage(message));
    }

    private static ChatSessionDto MapSession(
        ChatSession session,
        string? lastMessageContent)
    {
        return new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            LastMessagePreview = CreatePreview(lastMessageContent)
        };
    }

    private static ChatMessageDto MapMessage(ChatMessage message)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            ChatSessionId = message.ChatSessionId,
            Role = message.Role,
            Content = message.Content,
            CreatedAt = message.CreatedAt
        };
    }

    private static string? CreatePreview(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        var normalized = content.Trim();
        return normalized.Length <= PreviewLength
            ? normalized
            : $"{normalized[..(PreviewLength - 3)]}...";
    }
}
