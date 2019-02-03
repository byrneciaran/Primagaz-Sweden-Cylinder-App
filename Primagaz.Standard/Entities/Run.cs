using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Primagaz.Standard.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Run : IMergeItem
    {
        [NotMapped]
        public string MergeId => RunNumber;

        [JsonProperty,Key]
        public string RunNumber { get; set; }

        [JsonProperty]
        public string SubscriberID { get; set; }

        [JsonProperty]
        public string ChildSubscriberID { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public DateTimeOffset? DeliveryDate { get; set; }

        [JsonProperty]
        public bool Closed { get; set; }

        [JsonProperty]
        public DateTimeOffset? EndTime { get; set; }

        [JsonProperty]
        public int NumberOfCalls { get; set; }

        public long Timestamp { get; set; }

        [JsonProperty]
        public bool DriverRun { get; set; }

    }
}
