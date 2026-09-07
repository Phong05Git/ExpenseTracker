namespace ExpenseTracker.Domain.Exceptions;

public class NotFoundException : DomainExceptions
{
    public NotFoundException(string message) : base(message) { }
}