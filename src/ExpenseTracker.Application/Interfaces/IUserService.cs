using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Users;

namespace ExpenseTracker.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserProfileDto>> GetProfileAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Result<UserProfileDto>> UpdateProfileAsync(
        int userId,
        UpdateProfileDto request,
        CancellationToken cancellationToken = default);
}