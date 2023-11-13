using Bitbucket.Models;
using Bitbucket.Models.Interfaces;

namespace Bitbucket.Responces
{
    public class ShipmentResponse : IResponse
    {
        public object Value { get; set; }
        /// <summary>
        /// Execution time on application 
        /// </summary>
        public long Time { get; set; }
    }
}