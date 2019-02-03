using System;
using System.Threading.Tasks;
using Android.Accounts;
using Android.Content;
using Android.OS;
using Android.Util;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Primagaz.Standard;

namespace Primagaz.Android.Sync
{
    public class SyncAdapter : AbstractThreadedSyncAdapter
    {
        const string TAG = "Primagaz.SyncAdapter";
        readonly ContentResolver _contentResolver;

        public SyncAdapter(Context context, bool autoInitialize) : base(context, autoInitialize)
        {
            _contentResolver = context.ContentResolver;
        }

        public SyncAdapter(Context context, bool autoInitialize, bool allowParallelSyncs) : base(context, autoInitialize, allowParallelSyncs)
        {
            _contentResolver = context.ContentResolver;
        }

        public override void OnPerformSync(Account account, Bundle extras, string authority, ContentProviderClient provider, global::Android.Content.SyncResult syncResult)
        {
            Task.Run(async () =>
            {
                try
                {
                    Analytics.TrackEvent($"Sync Started At: {DateTime.Now}");

                    var response = await ApiClient.Instance.SyncCurrentSubscriptionAsync();

                    Analytics.TrackEvent($"Sync Completed At: {DateTime.Now}");

                    syncResult.Stats.NumIoExceptions = response.IsSuccessStatusCode ? 0 : 1;
                    syncResult.Stats.NumUpdates = response.IsSuccessStatusCode ? 1 : 0;
                }
                catch (Exception exception)
                {
                    Crashes.TrackError(exception);
                }
            });
        }

    }
}
