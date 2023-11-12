using Bitbucket.Models;
using Bitbucket.Models.Interfaces;

namespace Bitbucket.Responces
{
    public class ShipmentResponse : IResponse
    {
        public object Value { get; set; }
        public long Time { get; set; }
    }
}