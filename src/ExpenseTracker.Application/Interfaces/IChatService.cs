using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Chatbot;
using ExpenseTracker.Application.DTOs.Common;

namespace ExpenseTracker.Application.Interfaces;

public interface IChatService
{
    Task<Result<ChatSessionDto>> CreateSessionAsync(
        int userId,
        CreateChatSessionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResultDto<ChatSessionDto>>> GetSessionsAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResultDto<ChatMessageDto>>> GetMessagesAsync(
        int userId,
        int sessionId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Result<ChatMessageDto>> AddUserMessageAsync(
        int userId,
        int sessionId,
        SendChatMessageRequestDto request,
        CancellationToken cancellationToken = default);
}
