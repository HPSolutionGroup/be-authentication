namespace HP.Authentication.Application.CustomException
{
    public class UnAuthorizedException : ApplicationException
    {
        public UnAuthorizedException(string message)
            : base(message)
        {
        }

        public UnAuthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
