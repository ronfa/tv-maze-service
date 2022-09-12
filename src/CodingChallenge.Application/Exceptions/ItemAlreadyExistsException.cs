namespace CodingChallenge.Application.Exceptions;

public class ItemAlreadyExistsException : Exception
{
    public ItemAlreadyExistsException(string message)
        : base(message)
    {
    }
}
