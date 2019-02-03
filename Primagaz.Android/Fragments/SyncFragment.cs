using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Primagaz.Standard;
using Primagaz.Standard.Entities;
using Xamarin.Essentials;

namespace Primagaz.Android
{
    public class SyncFragment:BaseFragment
    {
        Button _syncButton;
        TextView _lastSyncTextView;
        TextView _statusTextView;
        public static readonly string TAG = typeof(SyncFragment).FullName;

        Repository _repository;
        Profile _profile;

        /// <summary>
        /// Destroy view
        /// </summary>
        public override void OnDestroyView()
        {
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
            _syncButton.Click -= OnSync;

            _repository.Dispose();
            base.OnDestroy();
        }

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        public static SyncFragment NewInstance()
        {
            var fragment = new SyncFragment { Arguments = new Bundle() };
            return fragment;
        }

        /// <summary>
        /// Create view
        /// </summary>
        /// <returns>The create view.</returns>
        /// <param name="inflater">Inflater.</param>
        /// <param name="container">Container.</param>
        /// <param name="savedInstanceState">Saved instance state.</param>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _repository = new Repository();

            var view = inflater.Inflate(Resource.Layout.sync_fragment, container, false);
            _syncButton = view.FindViewById<Button>(Resource.Id.syncButton);
            _lastSyncTextView = view.FindViewById<TextView>(Resource.Id.lastSyncTextView);
            _statusTextView = view.FindViewById<TextView>(Resource.Id.connectivityTextView);

            _profile = _repository.Profiles.First();

            Activity.ActionBar.Show();
            Activity.ActionBar.SetDisplayHomeAsUpEnabled(false);
            Activity.ActionBar.Title = Resources.GetString(Resource.String.nav_sync);

            _syncButton.Click += OnSync;
            Connectivity.ConnectivityChanged += OnConnectivityChanged;

            HasOptionsMenu = true;

            UpdateConnectionStatus();
            UpdateSyncTime();

            return view;
        }

        /// <summary>
        /// Select menu pops
        /// </summary>
        /// <returns><c>true</c>, if options item selected was oned, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            _fragmentActionListener.Pop();
            return true;
        }

        /// <summary>
        /// Profile property changed
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnProfilePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Activity.RunOnUiThread(UpdateSyncTime);
        }


        /// <summary>
        /// Connectivity changed event
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            Activity.RunOnUiThread(UpdateConnectionStatus);
        }

        /// <summary>
        /// Update sync time
        /// </summary>
        void UpdateSyncTime()
        {
            var lastUpdate = Resources.GetString(Resource.String.message_last_update);
            _lastSyncTextView.Text = $"{lastUpdate}: {_profile.LastSyncDate.ToLocalTime().ToString("dd/MM/yy HH:mm")}";
        }


        /// <summary>
        /// Updates the connection status.
        /// </summary>
        void UpdateConnectionStatus()
        {
            var current = Connectivity.NetworkAccess;
            var status = Resources.GetString(Resource.String.message_status);
            _statusTextView.Text = current == NetworkAccess.Internet ? $"{status}: Online" : $"{status}: Offline";
        }


        /// <summary>
        /// Sync event
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnSync(object sender, System.EventArgs e)
        {
            var title = Resources.GetString(Resource.String.message_loading);
            UserDialogs.Instance.ShowLoading(title);

            // disable sync button
            _syncButton.Enabled = false;

            using (var repo = new Repository())
            {
                var result = await ApiClient.Instance.SyncCurrentSubscriptionAsync();

                // close sync
                UserDialogs.Instance.HideLoading();

                // re-enable sync button
                _syncButton.Enabled = true;

                if (!result.IsSuccessStatusCode)
                {
                    var syncFailed = Resources.GetString(Resource.String.message_sync_failed);
                    UserDialogs.Instance.Alert(syncFailed);
                    return;
                }

                var syncSuccess = Resources.GetString(Resource.String.message_sync_success);
                Snackbar.Make(View, syncSuccess, Snackbar.LengthShort).Show();
                _fragmentActionListener.Pop();

            }

        }

    }
}
