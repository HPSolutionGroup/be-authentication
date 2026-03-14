namespace HP.Authentication.Domain.CustomException
{
    public class CustomException
    {
        public class InvalidDataException : Exception
        {
            public InvalidDataException() : base() { }
            public InvalidDataException(string message) : base(message) { }
            public InvalidDataException(string statuscode, string message) : base(message) { }
            public InvalidDataException(string message, Exception innerException) : base(message, innerException) { }

        }

        public class InternalServerErrorException : Exception
        {
            public InternalServerErrorException() : base() { }
            public InternalServerErrorException(string message) : base(message) { }
            public InternalServerErrorException(string message, Exception innerException) : base(message, innerException) { }

        }
        public class DataNotFoundException : Exception
        {
            public DataNotFoundException() : base() { }
            public DataNotFoundException(string message) : base(message) { }
            public DataNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        }
        public class DataExistException : Exception
        {
            public DataExistException() : base() { }
            public DataExistException(string message) : base(message) { }
            public DataExistException(string message, Exception innerException) : base(message, innerException) { }
        }

        public class UnAuthorizedException : Exception
        {
            public UnAuthorizedException() : base() { }
            public UnAuthorizedException(string message) : base(message) { }
            public UnAuthorizedException(string statuscode, string message) : base(message) { }
            public UnAuthorizedException(string message, Exception innerException) : base(message, innerException) { }
        }

        public class ForbbidenException : Exception
        {
            public ForbbidenException() : base() { }
            public ForbbidenException(string message) : base(message) { }
            public ForbbidenException(string statuscode, string message) : base(message) { }
            public ForbbidenException(string message, Exception innerException) : base(message, innerException) { }
        }
    }

}
