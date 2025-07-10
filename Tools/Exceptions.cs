namespace SuperFilter;

public class SuperFilterException : Exception
{
    public SuperFilterException(string message = "") : base(message)
    {
    }

    public SuperFilterException(string message, Exception innerException) : base(message, innerException)
    {
    }
}