namespace Superfilter;

public class SuperfilterException : Exception
{
    public SuperfilterException(string message = "") : base(message)
    {
    }

    public SuperfilterException(string message, Exception innerException) : base(message, innerException)
    {
    }
}