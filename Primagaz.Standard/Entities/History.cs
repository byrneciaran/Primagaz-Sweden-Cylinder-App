using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Primagaz.Standard.Entities
{
	public class History : IMergeItem
    {
        [NotMapped]
        public string MergeId => Id;

        [Key]
        public string Id { get; set; }
        public string CustomerAccountNumber { get; set; }
        public string ProductCode { get; set; }
    }

}
