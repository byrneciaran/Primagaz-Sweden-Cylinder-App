using System.Collections.Generic;
using Primagaz.Standard.Entities;

namespace Primagaz.Standard
{
    public class UploadRequest
    {
        public SyncLog SyncLog { get; set; }
        public MobileDevice MobileDevice { get; set; }
        public List<Run> Runs { get; set; }
        public List<Call> Calls { get; set; }
        public List<DriverStock> DriverStock { get; set; }
        public List<Order> Orders { get; set; }
        public List<DeliveryDocket> DeliveryDockets { get; set; }
        public List<NonDelivery> NonDeliveries { get; set; }
        public List<string> CompletedOrders { get; set; }
        public List<string> ClosedRuns { get; set; }
    }
}