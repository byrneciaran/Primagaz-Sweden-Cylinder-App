
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Primagaz.Standard.Entities
{
    public class OrderItem : IMergeItem
    {
        [NotMapped]
        public string MergeId => Id;
        
        [Key]
        public string Id { get; set; }
        public string OrderNumber { get; set; }
        public string RunNumber { get; set; }
        public string CustomerAccountNumber { get; set; }
        public string ProductCode { get; set; }
        public string ShortDescription { get; set; }
        public int Quantity { get; set; }
    }
}
