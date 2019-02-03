using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Primagaz.Standard.Entities
{
    public class NonDeliveryReason :IMergeItem
    {
        [NotMapped]
        public string MergeId => Id.ToString();

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int? SortOrder { get; set; }
    }
}