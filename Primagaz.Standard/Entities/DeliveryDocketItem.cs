using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Primagaz.Standard.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DeliveryDocketItem
    {
        [Key]
        public string Id { get; set; }

        [JsonProperty]
        public string DeliveryDocketID { get; set; }
        [JsonProperty]
        public string ProductCode { get; set; }
        [JsonProperty]
        public string SubscriberID { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public string RunNumber { get; set; }
        [JsonProperty]
        public string CustomerAccountNumber { get; set; }
        [JsonProperty]
        public int FullsDelivered { get; set; }
        [JsonProperty]
        public int EmptiesDelivered { get; set; }
        [JsonProperty]
        public int FullsCollected { get; set; }
        [JsonProperty]
        public int EmptiesCollected { get; set; }
        [JsonProperty]
        public int FaultyFulls { get; set; }
        [JsonProperty]
        public int FaultyEmpties { get; set; }
        [JsonProperty]
        public int OrderQuantity { get; set; }
        [JsonProperty]
        public string OrderNumber { get; set; }
        [JsonProperty]
        public string OrderReference { get; set; }
        [JsonProperty]
        public int Sequence { get; set; }

        public bool HasValue
        {
            get
            {
                return FullsDelivered > 0 
                                     || EmptiesDelivered > 0
                                     || FaultyFulls > 0 
                                     || FaultyEmpties > 0 
                                     || FullsCollected > 0 
                                     || EmptiesCollected > 0;
            }
        }


    }
}
