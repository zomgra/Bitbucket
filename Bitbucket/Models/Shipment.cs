namespace Bitbucket.Models
{
    public class Shipment
    {
        public Shipment(string barcode)
        {
            Barcode = barcode;
        }
        public int Id { get; set; }
        public string Barcode { get; set; }
    }
}