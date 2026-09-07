using AutoMapper;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Users;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class UserService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IUserService
{
    public async Task<Result<UserProfileDto>> GetProfileAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user == null)
            return Result<UserProfileDto>.Failure(
                "User not found.");

        return Result<UserProfileDto>.Success(
            mapper.Map<UserProfileDto>(user));
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(
        int userId,
        UpdateProfileDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user == null)
            return Result<UserProfileDto>.Failure(
                "User not found.");

        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (existingUser != null && existingUser.Id != userId)
        {
            return Result<UserProfileDto>.Failure(
                "Email already exists.");
        }

        user.UpdateProfile(
            request.FullName.Trim(),
            email);

        userRepository.Update(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserProfileDto>.Success(
            mapper.Map<UserProfileDto>(user));
    }
}