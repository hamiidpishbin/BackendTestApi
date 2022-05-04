namespace BackendTest.Services;

public class NotInsertedException : Exception
{
    public NotInsertedException()
    {
        
    }

    public NotInsertedException(string message)
    : base(message)
    {
        
    }

    public NotInsertedException(string message, Exception inner)
    : base(message, inner)
    {
        
    }
}