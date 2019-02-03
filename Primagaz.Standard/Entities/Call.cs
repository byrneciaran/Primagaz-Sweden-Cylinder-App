using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Primagaz.Standard.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Call : IMergeItem
    {
        [NotMapped]
        public string MergeId => Id;

        [JsonProperty, Key]
        public string Id { get; set; }
        [JsonProperty]
        public string CustomerAccountNumber { get; set; }
        [JsonProperty]
        public string ShortName { get; set; }
        [JsonProperty]
        public string CustomerName1 { get; set; }
        [JsonProperty]
        public string Address1 { get; set; }
        [JsonProperty]
        public string Address2 { get; set; }
        [JsonProperty]
        public string Address3 { get; set; }
        [JsonProperty]
        public string Address4 { get; set; }
        [JsonProperty]
        public string PostCode { get; set; }
        [JsonProperty]
        public bool LendingStatus { get; set; }
        [JsonProperty]
        public string RunNumber { get; set; }
        [JsonProperty]
        public bool? OnStop { get; set; }
        [JsonProperty]
        public string TelephoneNumber { get; set; }
        [JsonProperty]
        public string Directions1 { get; set; }
        [JsonProperty]
        public string Directions2 { get; set; }
        [JsonProperty]
        public float? Longitude { get; set; }
        [JsonProperty]
        public float? Latitude { get; set; }
        [JsonProperty]
        public bool NonDelivery { get; set; }
        [JsonProperty]
        public string NonDeliveryReason { get; set; }
        [JsonProperty]
        public string OrderNumber { get; set; }
        [JsonProperty]
        public string OrderType { get; set; }
        [JsonProperty]
        public int Sequence { get; set; }
        [JsonProperty]
        public bool Visited { get; private set; }
        [JsonProperty]
        public DateTimeOffset? VisitedDate { get; set; }
        [JsonProperty]
        public bool Removed { get; set; }

        public long Timestamp { get; set; }

        [JsonProperty]
        public bool DriverRun { get; set; }

        public void SetVisited(bool visited)
        {
            Visited = visited;
            VisitedDate = visited ? (DateTimeOffset?)DateTimeOffset.Now.ToLocalTime() : null;
        }

        [NotMapped]
        public bool HasDirections 
        {
            get {
                return Latitude.HasValue && Longitude.HasValue;
            }
        }

        [NotMapped]
        public string DirectionsUri
        {
            get
            {
                return $"google.navigation:q={Latitude.Value},{Longitude.Value}";
            }
        }

        [NotMapped]
        public string PhoneUri
        {
            get
            {
                return $"tel:{TelephoneNumber}";
            }
        }

        [NotMapped]
        public bool HasTelephoneNumber
        {
            get
            {
                return !String.IsNullOrWhiteSpace(TelephoneNumber);
            }
        }



    }
}