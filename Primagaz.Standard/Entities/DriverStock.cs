using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Primagaz.Standard.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DriverStock : IMergeItem
    {
        [NotMapped]
        public string MergeId => Id;

        [JsonProperty,Key]
        public string Id { get; set; }
        [JsonProperty]
        public string TrailerNumber { get; set; }
        [JsonProperty]
        public string ProductCode { get; set; }
        [JsonProperty]
        public string ShortDescription { get; set; }
        [JsonProperty]
        public string SubscriberID { get; set; }
        [JsonProperty]
        public int Fulls { get; set; }
        [JsonProperty]
        public int Empties { get; set; }
        [JsonProperty]
        public int FaultyFulls { get; set; }
        [JsonProperty]
        public int FaultyEmpties { get; set; }
        [JsonProperty]
        public float GrossWeight { get; set; }
        [JsonProperty]
        public float GallonsKilosPerFill { get; set; }
        [JsonProperty]
        public int Sequence { get; set; }

        public int? OrderQuantity { get; set; }

        public bool HasValue
        {
            get
            {
                return Fulls > 0 || Empties > 0
                            || FaultyFulls > 0 || FaultyEmpties > 0;
            }
        }

        public bool HasFulls
        {
            get
            {
                return Fulls > 0 || FaultyFulls > 0;
            }
        }


    }
}