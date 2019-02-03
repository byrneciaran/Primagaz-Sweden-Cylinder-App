using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace Primagaz.Standard.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DeliveryDocket
    {
        [JsonProperty, Key]
        public string DocketID { get; set; }
        [JsonProperty]
        public string SubscriberID { get; set; }
        [JsonProperty]
        public string ShortName { get; set; }
        [JsonProperty]
        public int CompanyTypeID { get; set; }
        [JsonProperty]
        public string CustomerAccountNumber { get; set; }
        [JsonProperty]
        public string DeviceIMEI { get; set; }
        [JsonProperty]
        public string DateModified { get; set; }
        [JsonProperty]
        public string PONumber { get; set; }
        [JsonProperty]
        public int DriverID { get; set; }
        [JsonProperty]
        public string RunNumber { get; set; }
        [JsonProperty]
        public byte[] CustomerSignature { get; set; }
        [JsonProperty]
        public string OrderReference { get; set; }
        [JsonProperty]
        public bool Invoice { get; set; }
        [JsonProperty]
        public string DriverPrintName { get; set; }
        [JsonProperty]
        public string CustomerPrintName { get; set; }
        [JsonProperty]
        public string ChildSubscriberID { get; set; }
        [JsonProperty]
        public string Memo { get; set; }
        [JsonProperty]
        public int DocketItems { get; set; }
        [JsonProperty]
        public string OrderNumber { get; set; }
        [JsonProperty]
        public int DocketVersion { get; set; }
        [JsonProperty]
        public int DocketNumber { get; set; }
        [JsonProperty]
        public bool ShowLendingStatus { get; set; }
        [JsonProperty]
        public bool Printed { get; set; }
        [JsonProperty]
        public double? GPSLatitude { get; set; }
        [JsonProperty]
        public double? GPSLongitude { get; set; }
        public bool Committed { get; set; }
        [JsonProperty]
        public bool Confirmed { get; set; }
        [JsonProperty]
        public bool Synced { get; set; }
        [JsonProperty]
        public bool SyncPending { get; set; }
        [JsonProperty]
        public int DeviceID { get; set; }
        [JsonProperty]
        public string CallID { get; set; }
        [JsonProperty]
        public string CustomerName1 { get; private set; }
        [JsonProperty]
        public string AddressLine1 { get; private set; }
        [JsonProperty]
        public string AddressLine2 { get; private set; }
        [JsonProperty]
        public string AddressLine3 { get; private set; }
        [JsonProperty]
        public string SignaturePath { get; set; }
        [JsonProperty]
        public string SubscriberDisplayName { get; private set; }
        [JsonProperty]
        public string SubscriberAddress { get; private set; }
        [JsonProperty]
        public string SubscriberPostCode { get; private set; }
        [JsonProperty]
        public string SubscriberCity { get; private set; }

        [JsonProperty,NotMapped]
        public List<DeliveryDocketItem> DeliveryDocketItems { get; set; }

        public string DocketPrefix {
            get
            {
                return DocketID.Split('-').First();
            }
        }

        public string DeviceNumber
        {
            get
            {
                return DocketPrefix.Substring(0,3);
            }
        }

        public void SetCall(Call call)
        {
            CallID = call.Id;
            CustomerName1 = call.CustomerName1;
            AddressLine1 = call.Address1;
            AddressLine3 = call.Address4;
            AddressLine2 = call.PostCode;
        }

        public void SetSubscriber(Subscriber subscriber)
        {
            SubscriberDisplayName = subscriber.DisplayName;
            SubscriberAddress = subscriber.Address;
            SubscriberPostCode = subscriber.PostCode;
            SubscriberCity = subscriber.City;
        }

        public string FormattedDateModified
        {
            get
            {
                return DateTimeOffset.ParseExact(DateModified, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
                                     .ToString("dd-MM-yyyy HH:mm");
            }
        }

        [NotMapped]
        public DateTimeOffset DateModifiedDate
        {
            get
            {
                return DateTimeOffset.ParseExact(DateModified, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
            }
        }

    }
}
