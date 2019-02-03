using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using Primagaz.Standard.Entities;
using CommonServiceLocator;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Xamarin.Essentials;
using System.IO.Compression;

namespace Primagaz.Standard
{
    public sealed class ApiClient
    {
        //const string BaseUrl = "http://handheld.primagaz.dk/api/sync";

        //const string BaseUrl = "http://192.168.1.103:3003/api/sync";

        const string BaseUrl = "https://mobile.primagaz.dk/api/sync";

        private const int TimeoutInMinutes = 2;

        readonly HttpClient _client;
        public EventHandler SyncComplete;
        JsonSerializer _serializer = new JsonSerializer();

        static List<string> _syncedDockets = new List<string>();
        static List<string> _syncedNonDeliveries = new List<string>();

        static bool _createProfile;

        public static ApiClient Instance { get; } = new ApiClient();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ApiClient()
        {
        }

        ApiClient()
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(TimeoutInMinutes)
            };

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        }

        /// <summary>
        /// Set the authorization header
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        void SetAuthorizationHeader(string username, string password)
        {
            var auth = new AuthenticationHeaderValue("Basic",
             Convert.ToBase64String(
             Encoding.UTF8.GetBytes($"{username}:{password}")));

            _client.DefaultRequestHeaders.Authorization = auth;
        }

        /// <summary>
        /// Serializes the sync request.
        /// </summary>
        /// <returns>The sync request.</returns>
        /// <param name="request">Request.</param>
        StringContent SerializeSyncRequest(UploadRequest request)
        {

            StringContent content = null;

            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(request, settings);
            content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return content;
        }

        /// <summary>
        /// Perform Initial Sync
        /// </summary>
        /// <returns>The initial sync async.</returns>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public async Task<SyncResult> PerfomInitialSyncAsync(string username, string password)
        {
            using (var repository = new Repository())
            {
                var device = repository.MobileDevices.FirstOrDefault();

                if (device == null)
                    device = new MobileDevice { DeviceID = Guid.NewGuid().ToString() };

                // flag to create a profile
                _createProfile = true;

                return await SynchroniseAsync(repository, username, password, BaseUrl, device).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sync the Current Subscription
        /// </summary>
        /// <returns>The current subscription async.</returns>
        public async Task<SyncResult> SyncCurrentSubscriptionAsync()
        {
            SyncResult syncResult = null;

            using (var repository = new Repository())
            {
                var profile = repository.Profiles.FirstOrDefault();

                if (profile == null)
                {
                    syncResult = new SyncResult { StatusCode = HttpStatusCode.OK };
                    return syncResult;
                }

                var device = repository.MobileDevices.FirstOrDefault();

                _createProfile = false;

                var url = string.Format("{0}/{1}", BaseUrl, device.Timestamp);
                syncResult = await SynchroniseAsync(repository, profile.SubscriberID,
                                                    profile.Password, url, device).ConfigureAwait(false);

                return syncResult;
            }
        }


        /// <summary>
        /// Synchronise Async
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="subscriberId">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="url">URL.</param>
        async Task<SyncResult> SynchroniseAsync(Repository repository, string subscriberId, string password, string url, MobileDevice device)
        {
            var deviceManager = ServiceLocator.Current.GetInstance<IDeviceManager>();
            device.UniqueDeviceID = deviceManager.GetUniqueDeviceId();

            deviceManager.LogEvent($"Sync In Progress At: {DateTime.Now}");

            // clear sync logs
            _syncedDockets.Clear();
            _syncedNonDeliveries.Clear();

            var profile = repository.Profiles.FirstOrDefault();

            var syncLog = new SyncLog
            {
                SubscriberID = subscriberId,
                AppVersion = AppInfo.VersionString,
                Build = AppInfo.BuildString,
                DeviceID = device.UniqueDeviceID,
                Model = DeviceInfo.Model,
                Version = DeviceInfo.VersionString,
                Manufacturer = DeviceInfo.Manufacturer
            };

            var request = new UploadRequest
            {
                MobileDevice = device,
                SyncLog = syncLog
            };

            // get profile data
            if (profile != null)
            {

                // get runs
                request.Runs = repository.Runs
                    .Where(x => x.Timestamp >= profile.TimeStamp).ToList();

                // get calls
                request.Calls = repository.Calls
                    .Where(x => x.Timestamp >= profile.TimeStamp).ToList();

                var dockets = repository.DeliveryDockets
                                   .Where(x => x.Committed && !x.Synced)
                                   .ToList();

                dockets.ForEach(docket =>
                {
                    docket.DeliveryDocketItems = repository.DeliveryDocketItems
                        .Where(x => x.DeliveryDocketID == docket.DocketID)
                        .ToList();
                });

                // get dockets
                request.DeliveryDockets = dockets;

                _syncedDockets = request.DeliveryDockets
                                  .Select(x => x.DocketID)
                                  .ToList();

                // get non deliveries
                request.NonDeliveries = repository.NonDeliveries
                                        .ToList();

                _syncedNonDeliveries = request.NonDeliveries
                                              .Select(x => x.Id)
                                              .ToList();

                // get driver stock
                request.DriverStock = repository.DriverStock
                                    .ToList();

                // get completed orders
                request.CompletedOrders = repository.Orders
                                        .Where(x => x.Completed)
                                        .Select(x => x.OrderNumber)
                                        .ToList();

                request.Orders = repository.Orders
                    .Where(x => x.Completed).ToList();

                if (request.DeliveryDockets.Any())
                    request.MobileDevice.LatestDocketID = request.DeliveryDockets
                        .OrderByDescending(x => x.DateModifiedDate)
                        .First().DocketID;

                request.MobileDevice.SyncDate = DateTime.Now.ToLocalTime();

            }

            // serialize sync request
            var content = SerializeSyncRequest(request);

            // set the authorization header
            SetAuthorizationHeader(subscriberId, password);

            SyncResult syncResult;

            try
            {

                using (var response = await _client.PostAsync(url, content).ConfigureAwait(false))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                using (var json = new JsonTextReader(reader))
                {
                    var dict = new Dictionary<string, string>
                    {
                        { "User", subscriberId },
                        { "Url", url },
                        { "Status Code", response.StatusCode.ToString() },
                        { "Reason Phrase", response.ReasonPhrase }
                    };

                    syncResult = new SyncResult
                    {
                        IsSuccessStatusCode = response.IsSuccessStatusCode,
                        ReasonPhrase = response.ReasonPhrase,
                        StatusCode = response.StatusCode
                    };

                    deviceManager.LogEvent($"Sync Completed At: {DateTime.Now} With Status: {response.StatusCode}");

                    if (syncResult.IsSuccessStatusCode)
                    {
                        syncResult.DownloadResponse = _serializer
                                .Deserialize<DownloadResponse>(json);

                        // store the password
                        syncResult.Password = password;

                        Merge(syncResult);
                    }
                }


                //var response = await _client.PostAsync(url, content).ConfigureAwait(false);

                //var dict = new Dictionary<string, string>
                //{
                //    { "User", subscriberId },
                //    { "Url", url },
                //    { "Status Code", response.StatusCode.ToString() },
                //    { "Reason Phrase", response.ReasonPhrase }
                //};

                //deviceManager.LogEvent("SynchroniseAsync", dict);

                //syncResult = new SyncResult
                //{
                //    IsSuccessStatusCode = response.IsSuccessStatusCode,
                //    ReasonPhrase = response.ReasonPhrase,
                //    StatusCode = response.StatusCode
                //};

                //if (syncResult.IsSuccessStatusCode)
                //{
                //    using (var stream = await response.Content.ReadAsStreamAsync())
                //    using (var reader = new StreamReader(stream))
                //    using (var json = new JsonTextReader(reader))
                //    {
                //        syncResult.DownloadResponse = _serializer
                //            .Deserialize<DownloadResponse>(json);

                //        // store the password
                //        syncResult.Password = password;

                //        Merge(syncResult);

                //    }
                //}

                return syncResult;

            }
            catch (Exception exception)
            {

                if (exception.InnerException != null)
                    System.Diagnostics.Debug.WriteLine($"EXCEPTION: {exception.InnerException.Message}");
                else
                    System.Diagnostics.Debug.WriteLine($"EXCEPTION: {exception.Message}");

                deviceManager.LogException(exception);

                syncResult = new SyncResult()
                {
                    IsSuccessStatusCode = false,
                    StatusCode = HttpStatusCode.RequestTimeout
                };

                return syncResult;
            }
        }


        void Merge(SyncResult response)
        {
            using (var repository = new Repository())
            {
                var tasks = new List<Task>();

                // get all syncing pending dockets
                var dockets = repository.DeliveryDockets
                                       .Where(x => _syncedDockets.Contains(x.DocketID))
                                       .ToList();

                // update the status to synced
                dockets.ForEach(docket =>
                {
                    docket.Synced = true;
                });

                // remove dockets older than 24 hours ago that have been 
                // committed and synced
                var oldDockets = dockets.Where(x =>
                {
                    var timespan = DateTimeOffset.UtcNow.ToLocalTime().Subtract(x.DateModifiedDate);
                    return timespan.Hours > 36 && x.Committed && x.Synced;

                }).ToList();

                // remove the old dockets
                oldDockets.ForEach(docket =>
                {
                    var docketItems = repository.DeliveryDocketItems
                                               .Where(x => x.DeliveryDocketID == docket.DocketID)
                                               .ToList();

                    repository.RemoveRange(docketItems);

                    if (File.Exists(docket.SignaturePath))
                        File.Delete(docket.SignaturePath);

                    repository.Remove(docket);
                });


                // remove non deliveries 
                var nonDeliveries = repository.NonDeliveries
                                             .Where(x => _syncedNonDeliveries.Contains(x.Id))
                                             .ToList();

                repository.RemoveRange(nonDeliveries);


                Merge(response.DownloadResponse.Runs, repository, false);
                Merge(response.DownloadResponse.Calls, repository, false);
                Merge(response.DownloadResponse.LendingStatus, repository, true);
                Merge(response.DownloadResponse.Orders, repository, true);
                Merge(response.DownloadResponse.OrderItems, repository, true);
                Merge(response.DownloadResponse.Products, repository, true);
                Merge(response.DownloadResponse.NonDeliveryReasons, repository, true);
                Merge(response.DownloadResponse.Trailers, repository, true);
                Merge(response.DownloadResponse.History, repository, true);
                Merge(response.DownloadResponse.DriverStock, repository, true);
                Merge(response.DownloadResponse.Customers, repository, true);

                // if the call has an order, then it is not visited
                var calls = repository.Calls.Where(x => x.OrderNumber != null).ToList();

                calls.ForEach(call =>
                {
                    call.SetVisited(false);
                });

                // sort calls - there is no guarantee that the seq. we get is going to be
                // unique. This causes problems when moving calls with the same seq. 
                // to prevent this we order calls by seq. but then set the seq. to the index
                var runs = repository.Runs.ToList();

                runs.ForEach((run) =>
                {
                    // order by sequence initially
                    calls = repository.Calls.Where(x => x.RunNumber == run.RunNumber)
                                 .OrderBy(x => x.VisitedDate)
                                 .ThenBy(x => x.Sequence)
                                 .ToList();

                    // then by array index so we have a unique sort order
                    for (var i = 0; i < calls.Count; i++)
                    {
                        var call = calls[i];
                        call.Sequence = i;
                    }
                });


                var modifiedDevice = response.DownloadResponse.MobileDevice;
                var existingDevice = repository.MobileDevices.FirstOrDefault(x => x.UniqueDeviceID == modifiedDevice.UniqueDeviceID);

                if (existingDevice == null)
                    repository.MobileDevices.Add(modifiedDevice);
                else
                    repository.Entry(existingDevice).CurrentValues.SetValues(modifiedDevice);

                var modifiedSubscriber = response.DownloadResponse.Subscriber;
                var existingSubscriber = repository.Subscribers.FirstOrDefault(x => x.ID == modifiedSubscriber.ID);

                if (existingSubscriber == null)
                {
                    var subscribers = repository.Subscribers;
                    repository.RemoveRange(subscribers);

                    repository.Subscribers.Add(modifiedSubscriber);
                }
                else
                    repository.Entry(existingSubscriber).CurrentValues.SetValues(modifiedSubscriber);


                Profile profile;

                if (_createProfile)
                {
                    var profiles = repository.Profiles;
                    repository.RemoveRange(profiles);

                    profile = new Profile
                    {
                        ParentSubscriberID = modifiedSubscriber.ParentSubscriberID,
                        Password = response.Password,
                        SubscriberID = modifiedSubscriber.ID,
                        DriverID = modifiedSubscriber.DriverID,
                        TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        LastSyncDate = DateTimeOffset.UtcNow,
                        EditRuns = modifiedSubscriber.EditRuns
                    };

                    repository.Add(profile);
                }

                // update the profile timestamp if it exists
                profile = repository.Profiles.FirstOrDefault();

                if (profile != null)
                {
                    profile.TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    profile.LastSyncDate = DateTimeOffset.UtcNow;
                }

                repository.SaveChanges();
            }

        }


        void Merge<T>(List<T> data, DbContext context, bool replace) where T : class
        {

            if (replace)
            {
                var removeRows = context.Set<T>();
                context.RemoveRange(removeRows);
                context.AddRange(data);
                return;
            }

            // get our existing rows
            var entities = context.Set<T>();

            var existingRows = entities.Cast<IMergeItem>().ToList();
            var modifiedRows = data.Cast<IMergeItem>().ToList();

            // iterate over each modified call
            modifiedRows.ForEach(modifiedRow =>
            {
                // check if the call exists
                var existingRow = existingRows.FirstOrDefault(x => x.MergeId == modifiedRow.MergeId);

                if (existingRow != null)
                    context.Entry(existingRow).CurrentValues.SetValues(modifiedRow);
                else
                    context.Add(modifiedRow);
            });

            var modifiedKeys = modifiedRows.Select(x => x.MergeId).ToList();

            var removedCalls = existingRows.Where(x => !modifiedKeys.Contains(x.MergeId));
            context.RemoveRange(removedCalls);

        }


    }

    public class JsonContent : HttpContent
    {
        private readonly JsonSerializer _serializer = new JsonSerializer();
        private readonly object _value;

        public JsonContent(object value)
        {
            _value = value;
            Headers.ContentType = new MediaTypeHeaderValue("application/gzip");
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var gzip = new GZipStream(stream, CompressionMode.Compress, true))
                using (var writer = new StreamWriter(gzip))
                {
                    _serializer.NullValueHandling = NullValueHandling.Ignore;
                    _serializer.Serialize(writer, _value);
                }
            });
        }
    }

}