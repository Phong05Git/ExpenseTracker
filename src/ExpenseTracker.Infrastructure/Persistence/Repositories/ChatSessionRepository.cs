using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class ChatSessionRepository(ApplicationDbContext context)
    : Repository<ChatSession>(context), IChatSessionRepository
{
    public async Task<IReadOnlyList<ChatSession>> GetByUserIdAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Messages
                .OrderByDescending(message => message.CreatedAt)
                .ThenByDescending(message => message.Id)
                .Take(1))
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<ChatSession?> GetOwnedByIdAsync(
        int sessionId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            x => x.Id == sessionId && x.UserId == userId,
            cancellationToken);
    }
}
