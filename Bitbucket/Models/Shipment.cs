using System.ComponentModel.DataAnnotations;

namespace Bitbucket.Models
{
    public class Shipment
    {
        public Shipment(string barcode)
        {
            Barcode = barcode;
        }
        public Shipment()
        {

        }
        [Key]
        public string Barcode { get; set; }
    }
}