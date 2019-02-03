using System.ComponentModel.DataAnnotations;

namespace Primagaz.Standard.Entities
{
    public class Printer
    {
        [Key]
        public string Address { get; set; }
        public string FriendlyName { get; set; }
    }

}
