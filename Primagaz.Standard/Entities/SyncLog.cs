using System;

namespace Primagaz.Standard.Entities
{
    public class SyncLog
    {
        public string AppVersion { get; set; }
        public string Build { get; set; }
        public string SubscriberID { get; set; }
        public string Manufacturer { get; set; }
        public string DeviceID { get; set; }
        public string Model { get; set; }
        public string Version { get; set; }
        public string StartSyncDate { get; set; }

        public SyncLog()
        {
            StartSyncDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
        }
    }
}
