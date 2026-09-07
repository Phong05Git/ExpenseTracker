namespace ExpenseTracker.Domain.Exceptions;

public class ValidationException : DomainExceptions
{
    public ValidationException(string message) : base(message) { }
}