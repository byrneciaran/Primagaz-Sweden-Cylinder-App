using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Primagaz.Standard.Entities
{
    public class Trailer : IMergeItem
    {
        [NotMapped]
        public string MergeId => TrailerNumber;

        [Key]
        public string TrailerNumber { get; set; }
        public string Description { get; set; }
    }
}