namespace HP.Authentication.Application.CustomException
{
    public abstract class ApplicationException : Exception
    {
        protected ApplicationException(string message): base(message) { }

        protected ApplicationException(string message, Exception innerException): base(message, innerException) { }
    }
}
