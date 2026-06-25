namespace HP.Authentication.Application.CustomException
{
    public class DataExistException : ApplicationException
    {
        public DataExistException(string message)
            : base(message)
        {
        }

        public DataExistException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
