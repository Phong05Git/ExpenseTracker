using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Domain.Interfaces;

public interface IChatSessionRepository : IRepository<ChatSession>
{
    Task<IReadOnlyList<ChatSession>> GetByUserIdAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<ChatSession?> GetOwnedByIdAsync(
        int sessionId,
        int userId,
        CancellationToken cancellationToken = default);
}
