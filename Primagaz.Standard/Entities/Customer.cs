using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Primagaz.Standard.Entities
{
    public class Customer : IMergeItem
    {
        [NotMapped]
        public string MergeId => CustomerAccountNumber;

        [Key]
        public string CustomerAccountNumber { get; set; }
        public string ShortName { get; set; }
        public string CustomerName1 { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string PostCode { get; set; }
        public bool OnStop { get; set; }
        public bool? LendingStatus { get; set; }
        public string TelephoneNumber { get; set; }
        public string Directions1 { get; set; }
        public string Directions2 { get; set; }
        public float? Longitude { get; set; }
        public float? Latitude { get; set; }

        [NotMapped]
        public bool Selected { get; set; }

        [NotMapped]
        public bool HasOrder {
            get {
                return !String.IsNullOrWhiteSpace(OrderNumber);
            }
        }

        [NotMapped]
        public string OrderNumber { get; set; }
        [NotMapped]
        public string OrderType { get; set; }
    }
}
