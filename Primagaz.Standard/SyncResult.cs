using System.Collections.Generic;
using System.Net;
using Primagaz.Standard.Entities;

namespace Primagaz.Standard
{
    public class SyncResult
    {
        public bool IsSuccessStatusCode { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public DownloadResponse DownloadResponse { get; set; }
        public string Password { get; set; }

        public string Description
        {
            get
            {
                switch (StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return "Invalid username or password.";
                    default:
                        return "There was a problem connection to HandyForm. Check your Internet connection and try again.";
                }
            }
        }
    }

    public class DownloadResponse
    {
        public List<Order> Orders { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public List<Customer> Customers { get; set; }
        public List<History> History { get; set; }
        public List<NonDeliveryReason> NonDeliveryReasons { get; set; }
        public List<Product> Products { get; set; }
        public List<Trailer> Trailers { get; set; }
        public List<DriverStock> DriverStock { get; set; }
        public List<LendingStatus> LendingStatus { get; set; }
        public List<Run> Runs { get; set; }
        public List<Call> Calls { get; set; }
        public MobileDevice MobileDevice { get; set; }
        public Subscriber Subscriber { get; set; }
        public long Timestamp { get; set; }
    }
}