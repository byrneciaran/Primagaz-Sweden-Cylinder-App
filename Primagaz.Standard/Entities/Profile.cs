using System;
using System.ComponentModel.DataAnnotations;

namespace Primagaz.Standard.Entities
{
    public class Profile
    {
        [Key]
        public string SubscriberID { get; set; }
        public string ParentSubscriberID { get; set; }
        public string CurrentRunNumber { get; set; }
        public string CurrentTrailerNumber { get; set; }
        public string CurrentDocketID { get; set; }
        public string Password { get; set; }
        public int DriverID { get; set; }
        public DateTimeOffset LastSyncDate { get; set; }
        public long TimeStamp { get; set; }
        public bool EditRuns { get; set; }
       
    }
}
