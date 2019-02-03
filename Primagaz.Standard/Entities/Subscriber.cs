using System.ComponentModel.DataAnnotations;

namespace Primagaz.Standard.Entities
{
    public class Subscriber
    {
        [Key]
        public string ID { get; set; }
        public string DisplayName { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public int DriverID { get; set; }
        public string ParentSubscriberID { get; set; }
        public bool EditRuns { get; set; }
        public long Timestamp { get; set; }
    }
}
