using Bitbucket.Responces;

namespace Bitbucket.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException()
        {
        }
        public DomainException(string message) : base(message)
        {
        }
    }
}