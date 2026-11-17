using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Domain.Interfaces;

public interface IChatMessageRepository : IRepository<ChatMessage>
{
    Task<IReadOnlyList<ChatMessage>> GetBySessionIdAsync(
        int sessionId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default);
}
