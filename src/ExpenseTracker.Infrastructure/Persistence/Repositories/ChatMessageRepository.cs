using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class ChatMessageRepository(ApplicationDbContext context)
    : Repository<ChatMessage>(context), IChatMessageRepository
{
    public async Task<IReadOnlyList<ChatMessage>> GetBySessionIdAsync(
        int sessionId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.ChatSessionId == sessionId)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .CountAsync(x => x.ChatSessionId == sessionId, cancellationToken);
    }
}
