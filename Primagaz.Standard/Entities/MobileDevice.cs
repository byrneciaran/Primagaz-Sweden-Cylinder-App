using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace Primagaz.Standard.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MobileDevice
    {
        [JsonProperty,Key]
        public int Id { get; set; }
        [JsonProperty]
        public string DeviceID { get; set; }
        [JsonProperty]
        public string UniqueDeviceID { get; set; }
        [JsonProperty]
        public int DocketNumber { get; set; }
        [JsonProperty]
        public int DocketVersion { get; set; }
        [JsonProperty]
        public int RunNumber { get; set; }
        [JsonProperty]
        public string PrinterAddress { get; set; }
        [JsonProperty]
        public string LatestDocketID { get; set; }
        [JsonProperty]
        public DateTimeOffset? SyncDate { get; set; }
        [JsonProperty]
        public long Timestamp { get; set; }
    }
}
