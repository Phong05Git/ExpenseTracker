namespace ExpenseTracker.Application.DTOs.Users;

public class UpdateProfileDto
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}