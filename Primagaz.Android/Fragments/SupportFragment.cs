using Android.OS;
using Android.Views;
using System.Linq;
using Android.Widget;
using Android.Net;
using Android.Content;
using Android.Content.PM;
using Android;
using Android.App;
using Android.Runtime;
using Acr.UserDialogs;
using System;
using Microsoft.AppCenter.Crashes;

using Uri = Android.Net.Uri;

namespace Primagaz.Android
{
    public class SupportFragment:BaseFragment
    {
        public static readonly string TAG = typeof(SupportFragment).FullName;
        const int CallPermissionResult = 999;

        Button _techicalSupportButton;
        Button _generalSupportButton;
        Button _uploadDatabaseButton;
        TextView _deviceIdTextView;

        const string TechnicalSupportNumber = "+4556643233";
        const string GeneralSupportNumber = "+4556643232";

        /// <summary>
        /// Destroy view
        /// </summary>
        public override void OnDestroyView()
        {
            _techicalSupportButton.Click -= OnTechnicalSupportClick;
            _generalSupportButton.Click -= OnGeneralSupportClick;
            _uploadDatabaseButton.Click -= OnUploadDatabaseClick;

            base.OnDestroy();
        }

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        public static SupportFragment NewInstance()
        {
            var fragment = new SupportFragment { Arguments = new Bundle() };
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

            var view = inflater.Inflate(Resource.Layout.support_fragment, container, false);

            Activity.ActionBar.Show();
            Activity.ActionBar.SetDisplayHomeAsUpEnabled(false);
            Activity.ActionBar.Title = Resources.GetString(Resource.String.nav_support);

            _techicalSupportButton = view.FindViewById<Button>(Resource.Id.technicalSupportButton);
            _techicalSupportButton.Click += OnTechnicalSupportClick;

            _generalSupportButton = view.FindViewById<Button>(Resource.Id.generalSupportButton);
            _generalSupportButton.Click += OnGeneralSupportClick;

            _uploadDatabaseButton = view.FindViewById<Button>(Resource.Id.uploadDatabaseButton);
            _uploadDatabaseButton.Click += OnUploadDatabaseClick;

            _deviceIdTextView = view.FindViewById<TextView>(Resource.Id.deviceIdTextView);

            var deviceManager = new DeviceManager();
            _deviceIdTextView.Text = $"Device: {deviceManager.GetUniqueDeviceId()}";

            HasOptionsMenu = true;

            return view;
        }

        /// <summary>
        /// Upload Database
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnUploadDatabaseClick(object sender, EventArgs e)
        {
            UserDialogs.Instance.ShowLoading("Uploading");
            _uploadDatabaseButton.Enabled = false;

            try
            {
                await SupportUtils.UploadDatabase();
                UserDialogs.Instance.Alert("Upload Complete");
            }
            catch(Exception exception)
            {
                UserDialogs.Instance.Alert("Upload Failed");
                Crashes.TrackError(exception);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                _uploadDatabaseButton.Enabled = true;
            }
        
 
        }

        /// <summary>
        /// General Support Click
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnGeneralSupportClick(object sender, System.EventArgs e)
        {
            if (!RequestCallPermission())
                return;

            MakeCall(GeneralSupportNumber);
        }

        /// <summary>
        /// Technical Support Click
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnTechnicalSupportClick(object sender, System.EventArgs e)
        {
            if (!RequestCallPermission())
                return;

            MakeCall(TechnicalSupportNumber);
        }

        /// <summary>
        /// Make the call
        /// </summary>
        /// <param name="phoneNumber">Phone number.</param>
        void MakeCall(string phoneNumber)
        {
            var uri = Uri.Parse($"tel:{phoneNumber}");
            var callIntent = new Intent(Intent.ActionCall, uri);
            StartActivity(callIntent);
        }


        /// <summary>
        /// Request permission result
        /// </summary>
        /// <param name="requestCode">Request code.</param>
        /// <param name="permissions">Permissions.</param>
        /// <param name="grantResults">Grant results.</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == CallPermissionResult)
            {
                if (!grantResults.Any() || grantResults.First() == Permission.Denied)
                    ShowRequestCallPermission();
            }
        }

        /// <summary>
        /// Requests the call permission.
        /// </summary>
        /// <returns><c>true</c>, if call permission was requested, <c>false</c> otherwise.</returns>
        bool RequestCallPermission()
        {
            // check the permission for the phone to make calls
            if (Activity.CheckSelfPermission(Manifest.Permission.CallPhone) == Permission.Granted)
                return true;

            // Provide an additional rationale to the user if the permission was not granted
            // and the user would benefit from additional context for the use of the permission.
            // For example if the user has previously denied the permission.
            if (ShouldShowRequestPermissionRationale(Manifest.Permission.CallPhone))
                ShowRequestCallPermission();
            else
                RequestPermissions(new string[] { Manifest.Permission.CallPhone }, CallPermissionResult);

            return false;
        }

        /// <summary>
        /// Shows the request call permission.
        /// </summary>
        void ShowRequestCallPermission()
        {
            var title = Resources.GetString(Resource.String.message_permission);
            var message = Resources.GetString(Resource.String.message_permissin_call);

            var builder = new AlertDialog.Builder(Activity);
            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetCancelable(false);
            builder.SetPositiveButton("Ok", (sender, e) =>
            {
                RequestPermissions(new string[] { Manifest.Permission.CallPhone }, CallPermissionResult);
            });

            builder.Show();
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
    }
}
