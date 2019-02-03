using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Primagaz.Standard.Entities;

namespace Primagaz.Standard.Service
{
    public static class DocketService
    {
        const int CompanyTypeId = 1;
        const string TimeZoneId = "Europe/Copenhagen";
        const int DeviceIdLength = 3;
        const int DocketNumberLength = 5;

        /// <summary>
        /// Create new docket
        /// </summary>
        /// <param name="realm">Realm.</param>
        /// <param name="runNumber">Run number.</param>
        /// <param name="call">Call.</param>
        /// <param name="subscriber">Subscriber.</param>
        public static void CreateNewDocket(Repository repository, string runNumber, Call call, Subscriber subscriber)
        {

            var device = repository.MobileDevices.First();
            var profile = repository.Profiles.First();
            var customer = repository.Customers.Find(call.CustomerAccountNumber);
            var lendingStatus = customer.LendingStatus.GetValueOrDefault();

            // remove any uncommited dockets or current dockets
            var uncommittedDockets = repository.DeliveryDockets
                                          .Where(x => !x.Committed)
                                          .ToList();

            uncommittedDockets.ForEach((uncommittedDocket) =>
            {
                // remove all docket items
                var uncommittedDocketItems = repository.DeliveryDocketItems
                                       .Where(x => x.DeliveryDocketID == uncommittedDocket.DocketID)
                                       .ToList();

                repository.RemoveRange(uncommittedDocketItems);
                repository.RemoveRange(uncommittedDockets);
            });

            repository.SaveChanges();

            // get the latest docket
            var latestDocket = repository.DeliveryDockets
                                    .OrderByDescending(x => x.DateModifiedDate)
                                    .FirstOrDefault();

            // get the docket number either from the latest docket or device table
            var latestDeviceDocketNumber = device.DocketNumber + 1;
            var docketNumber = latestDocket != null ? latestDocket.DocketNumber + 1 : latestDeviceDocketNumber;

            if (latestDeviceDocketNumber > docketNumber)
                docketNumber = latestDeviceDocketNumber;

            var docketId = CreateDocketID(1, device.Id, docketNumber);

            var docketExists = repository.DeliveryDockets.Any(x => x.DocketID == docketId);

            while (docketExists)
            {
                docketNumber = latestDocket.DocketNumber + 1;
                docketId = CreateDocketID(1, latestDocket.DeviceID, docketNumber);
                docketExists = repository.DeliveryDockets.Any(x => x.DocketID == docketId);
            }

            // we always want the date represented in local danish time for both 
            // print and saving agains the docket, as historical dates were not in UTC.
            var localTime = GetLocalTime();

            var docket = new DeliveryDocket
            {
                DateModified = localTime,
                RunNumber = runNumber,
                SubscriberID = subscriber.ParentSubscriberID,
                ChildSubscriberID = subscriber.ID,
                DriverPrintName = subscriber.ID,
                ShortName = call.ShortName,
                DriverID = profile.DriverID,
                CustomerAccountNumber = call.CustomerAccountNumber,
                CompanyTypeID = CompanyTypeId,
                DeviceIMEI = device.DeviceID.Substring(0, 13),
                DeviceID = device.Id,
                OrderNumber = call.OrderNumber,
                ShowLendingStatus = lendingStatus,
                DocketVersion = 1,
                DocketNumber = docketNumber,
                DocketID = docketId,
                Memo = docketId,
                Committed = false,
                Confirmed = false
            };




            // check that this docket id is unique
            var existing = repository.DeliveryDockets.Find(docket.DocketID);

            // docket exists - throw an exception
            if (existing != null)
                throw new DocketExistsException($"DocketID {docket.DocketID} " +
                                                $"already exists and was created on {docket.FormattedDateModified}");


            docket.SetCall(call);
            docket.SetSubscriber(subscriber);

            // get the products
            var products = repository.Products.ToList();

            // get the history
            var history = repository.History
                               .Where(x => x.CustomerAccountNumber == docket.CustomerAccountNumber).ToList();

            // flag any products previously ordered
            history.ForEach((record) =>
            {
                var product = products.FirstOrDefault(x => x.ProductCode == record.ProductCode);

                if (product != null)
                    product.PreviouslyOrdered = true;
            });

            // order the products by previously ordered & then sequence
            products = products.OrderByDescending(x => x.PreviouslyOrdered).ThenBy(x => x.Sequence).ToList();

            var orderItems = repository.OrderItems.Where(x => x.OrderNumber == call.OrderNumber);

            // add the products
            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];
                var sequence = product.Sequence.GetValueOrDefault();

                var docketItem = new DeliveryDocketItem
                {
                    SubscriberID = subscriber.ParentSubscriberID,
                    Description = product.ShortDescription,
                    ProductCode = product.ProductCode,
                    RunNumber = docket.RunNumber,
                    Sequence = i,
                    CustomerAccountNumber = docket.CustomerAccountNumber,
                    DeliveryDocketID = docket.DocketID,
                    Id = Guid.NewGuid().ToString()
                };

                var orderItem = orderItems.FirstOrDefault(x => x.ProductCode == docketItem.ProductCode);

                if (orderItem != null)
                {
                    docketItem.OrderQuantity = orderItem.Quantity;
                    docketItem.OrderNumber = orderItem.OrderNumber;
                }

                repository.DeliveryDocketItems.Add(docketItem);
            }

            repository.Add(docket);
            profile.CurrentDocketID = docket.DocketID;

            repository.SaveChanges();
        }



        /// <summary>
        /// Gets the local time.
        /// </summary>
        /// <returns>The local time.</returns>
        static string GetLocalTime()
        {
            return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        /// <summary>
        /// Updates the docket item.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="item">Item.</param>
        /// <param name="product">Product.</param>
        public static void UpdateDocketItem(Repository repository, DeliveryDocketItem item, Product product)
        {
            item.FullsDelivered = product.Fulls.GetValueOrDefault();
            item.EmptiesDelivered = product.EmptiesDelivered.GetValueOrDefault();
            item.FullsCollected = product.FullsCollected.GetValueOrDefault();
            item.EmptiesCollected = product.Empties.GetValueOrDefault();
            item.FaultyFulls = product.FaultyFulls.GetValueOrDefault();
            item.FaultyEmpties = product.FaultyEmpties.GetValueOrDefault();
            item.OrderQuantity = product.OrderQuantity.GetValueOrDefault();
            repository.SaveChanges();

        }


        /// <summary>
        /// Commits the docket.
        /// </summary>
        /// <returns>The docket.</returns>
        /// <param name="realm">Realm.</param>
        /// <param name="signature">Signature.</param>
        /// <param name="customerName">Customer print name.</param>
        /// <param name="confirm">If set to <c>true</c> confirm.</param>
        public static string CommitDocket(Repository repository, string docketId, byte[] signature,
                                        double? longitude, double? latitude,
                                          string customerName, bool confirm)
        {
            // update realm to most recent point

            var device = repository.MobileDevices.First();
            var profile = repository.Profiles.First();

            // get the current docket
            var docket = repository.DeliveryDockets.Find(docketId);

            if (docket == null)
                throw new ArgumentNullException($"DocketID {docketId} can not be null");

            // check if we have already committed this docket
            var alreadyCommitted = docket.Committed;

            // set the device docket number
            device.DocketNumber = docket.DocketNumber;

            // set confirmed
            docket.Confirmed = confirm;

            // docket is not confirmed but was already committed.
            // we need a new version of the docket
            if (!docket.Confirmed && alreadyCommitted)
            {
                docket = CreateNewDocketVersion(repository, docket);
                repository.DeliveryDockets.Add(docket);
            }

            // commit this docket
            docket.Committed = true;

            // store GPS
            docket.GPSLatitude = latitude;
            docket.GPSLongitude = longitude;

            // store signature
            docket.CustomerSignature = signature;
            docket.CustomerPrintName = customerName;

            // store items
            docket.Invoice = true;
            docket.DocketItems = repository.DeliveryDocketItems.Count(x => x.DeliveryDocketID == docket.DocketID);

            // if the docket is confirmed
            if (docket.Confirmed)
            {
                // if the docket has an order
                if (!String.IsNullOrWhiteSpace(docket.OrderNumber))
                {
                    // mark the order as complete
                    var order = repository.Orders.Find(docket.OrderNumber);

                    if (order != null)
                    {
                        order.DateModified = docket.DateModified;
                        order.Completed = true;
                    }
                }


                // get the call
                var call = repository.Calls.Find(docket.CallID);

                if (call != null)
                {
                    // get the last call on the run
                    var lastCallOnRun = repository.Calls
                                             .OrderBy(x => x.Sequence)
                                             .LastOrDefault(x => x.RunNumber == docket.RunNumber);

                    call.Sequence = lastCallOnRun.Sequence + 1;
                    call.SetVisited(true);
                    call.NonDelivery = false;
                    call.NonDeliveryReason = null;
                    call.OrderNumber = null;
                    call.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }


                // update the trailer stock
                var driverStock = repository.DriverStock
                                       .Where(x => x.TrailerNumber == profile.CurrentTrailerNumber);

                // get the order items
                var orderItems = repository.OrderItems
                                      .Where(x => x.RunNumber == profile.CurrentRunNumber);

                var docketItems = repository.DeliveryDocketItems.Where(x => x.DeliveryDocketID == docket.DocketID);

                // update the trailer stock
                foreach (var docketItem in docketItems)
                {
                    var stockItem = driverStock
                        .FirstOrDefault(x => x.ProductCode == docketItem.ProductCode);

                    if (stockItem != null)
                    {
                        // update order quantity
                        stockItem.OrderQuantity = orderItems.Where(x => x.ProductCode == stockItem.ProductCode)
                                                            .ToList()
                                                            .Sum(x => x.Quantity);

                        // update fulls stock
                        stockItem.Fulls = stockItem.Fulls - docketItem.FullsDelivered + docketItem.FullsCollected;

                        if (stockItem.Fulls < 0)
                            stockItem.Fulls = 0;

                        stockItem.Empties = stockItem.Empties + docketItem.EmptiesCollected - docketItem.EmptiesDelivered;

                        if (stockItem.Empties < 0)
                            stockItem.Empties = 0;

                        stockItem.FaultyFulls = stockItem.FaultyFulls + docketItem.FaultyFulls;
                        stockItem.FaultyEmpties = stockItem.FaultyEmpties + docketItem.FaultyEmpties;
                    }
                }

                // remove docket items that have no value
                var redundantDocketItems = repository.DeliveryDocketItems.Where(x => x.FullsDelivered == 0 && x.EmptiesDelivered == 0
                                                                                 && x.FaultyFulls == 0 && x.FaultyEmpties == 0
                                                                                 && x.FullsCollected == 0 && x.EmptiesCollected == 0);

                repository.DeliveryDocketItems.RemoveRange(redundantDocketItems);
            }

            repository.SaveChanges();


            return docket.DocketID;
        }

        /// <summary>
        /// Sets the docket order reference.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="docket">Docket.</param>
        /// <param name="orderReference">Order reference.</param>
        public static void SetDocketOrderReference(Repository repository, DeliveryDocket docket, string orderReference)
        {
            docket.OrderReference = orderReference;
            repository.SaveChanges();
        }

        static string CreateDocketID(int version, int deviceId, int docketNumber)
        {
            var versionString = version.ToString().PadLeft(2, '0');

            var prefix = new StringBuilder(deviceId.ToString().PadLeft(DeviceIdLength, '0'));
            prefix.Append(docketNumber.ToString().PadLeft(DocketNumberLength, '0'));

            return $"{prefix}-{versionString}";
        }

        /// <summary>
        /// Clone Docket
        /// </summary>
        /// <returns>The docket.</returns>
        /// <param name="docket">Docket.</param>
        static DeliveryDocket CreateNewDocketVersion(Repository repository,DeliveryDocket docket)
        {
            // copy docket through serialization
            var json = JsonConvert.SerializeObject(docket);
            var newDocket = JsonConvert.DeserializeObject<DeliveryDocket>(json);

            // set the changed properties
            newDocket.DateModified = GetLocalTime();

            // increment the docket version
            int version = newDocket.DocketVersion + 1;
            int docketNumber = newDocket.DocketNumber;

            var docketId = CreateDocketID(version, docket.DeviceID, newDocket.DocketNumber);

            var docketExists = repository.DeliveryDockets.Any(x => x.DocketID == docketId);

            while (docketExists)
            {
                version = version + 1;
                docketId = CreateDocketID(version, docket.DeviceID, newDocket.DocketNumber);
                docketExists =repository.DeliveryDockets.Any(x => x.DocketID == docketId);
            }

            // create new docket id
            newDocket.DocketVersion = version;
            newDocket.DocketID = docketId;
            newDocket.Memo = docketId;

            var existingDocketItems = repository.DeliveryDocketItems.Where(x => x.DeliveryDocketID == docket.DocketID);

            // update docket id for delivery items
            foreach (var existingDocketItem in existingDocketItems)
            {
                json = JsonConvert.SerializeObject(existingDocketItem);
                var newDocketItem = JsonConvert.DeserializeObject<DeliveryDocketItem>(json);

                newDocketItem.Id = Guid.NewGuid().ToString();
                newDocketItem.DeliveryDocketID = docketId;

                repository.DeliveryDocketItems.Add(newDocketItem);
            }

            return newDocket;
        }
    }
}

class DocketExistsException : Exception
{
    public DocketExistsException(string message) : base(message)
    {
    }
}
