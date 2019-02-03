using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Primagaz.Standard.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Order : IMergeItem
    {
        [NotMapped]
        public string MergeId => OrderNumber;

        [JsonProperty,Key]
        public string OrderNumber { get; set; }
        public string CustomerAccountNumber { get; set; }
        public string RunNumber { get; set; }
        public string OrderType { get; set; }
        public DateTimeOffset? DeliveryDate { get; set; }
        [JsonProperty]
        public bool Completed { get; set; }
        [JsonProperty]
        public bool NonDelivery { get; set; }
        [JsonProperty]
        public string DateModified { get; set; }
    }


}
