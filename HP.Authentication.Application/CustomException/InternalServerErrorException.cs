namespace HP.Authentication.Application.CustomException
{
    public class InternalServerErrorException : ApplicationException
    {
        public InternalServerErrorException(string message)
            : base(message)
        {
        }

        public InternalServerErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
