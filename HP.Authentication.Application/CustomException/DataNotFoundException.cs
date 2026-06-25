namespace HP.Authentication.Application.CustomException
{
    public class DataNotFoundException : ApplicationException
    {
        public DataNotFoundException(string message)
            : base(message)
        {
        }

        public DataNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
