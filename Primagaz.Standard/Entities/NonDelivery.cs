using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Primagaz.Standard.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NonDelivery
    {
        [JsonProperty,Key]
        public string Id { get; set; }
        [JsonProperty]
        public string CustomerAccountNumber { get; set; }
        [JsonProperty]
        public string SubscriberID { get; set; }
        [JsonProperty]
        public string RunNumber { get; set; }
        [JsonProperty]
        public string DateModified { get; set; }
        [JsonProperty]
        public int NonDeliveryReasonID { get; set; }
        [JsonProperty]
        public string OrderNumber { get; set; }

    }
}
