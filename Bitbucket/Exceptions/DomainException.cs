using Bitbucket.Responces;

namespace Bitbucket.Exceptions
{
    public class DomainException : Exception
    {
        public int StatusCode { get; set; }
        public DomainException()
        {
        }
        public DomainException(string message, int status = 500) : base(message)
        {
            StatusCode = status;
        }
    }
}