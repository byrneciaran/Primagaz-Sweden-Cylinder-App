using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Primagaz.Standard.Entities
{
    public class Product : IMergeItem
    {
        [NotMapped]
        public string MergeId => ProductCode;

        [Key]
        public string ProductCode { get; set; }
        public string ShortDescription { get; set; }
        public float GallonsKilosPerFill { get; set; }
        public float GrossWeight { get; set; }
        public int? Sequence { get; set; }

        [NotMapped]
        public int? OrderQuantity { get; set; }
        [NotMapped]
        public int? Fulls { get; set; }
        [NotMapped]
        public int? Empties { get; set; }
        [NotMapped]
        public int? FaultyFulls { get; set; }
        [NotMapped]
        public int? FaultyEmpties { get; set; }
        [NotMapped]
        public int? FullsCollected { get; set; }
        [NotMapped]
        public int? EmptiesDelivered { get; set; }
        [NotMapped]
        public bool PreviouslyOrdered { get; set; }
    }

}
