using Android.OS;
using Android.Views;
using System.IO;
using Primagaz.Standard.Entities;
using Xamarin.Controls;
using System;
using Android.Widget;
using Primagaz.Standard;
using System.Linq;
using Acr.UserDialogs;
using Primagaz.Standard.Service;
using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.Runtime;
using Xamarin.Essentials;
using Android.Graphics;
using Microsoft.AppCenter.Crashes;

using Bitmap = Android.Graphics.Bitmap;
using Environment = Android.OS.Environment;

namespace Primagaz.Android
{
    public class SignatureFragment : BaseFragment
    {
        const int LocationPermissionResult = 999;
        const int StoragePermissionResult = 1000;
        const int Retries = 3;
        public static readonly string TAG = typeof(SignatureFragment).FullName;

        string _docketId;
        SignaturePadView _signatureView;
        Button _confirmButton;
        EditText _customerNameEditText;
        Location _location;

        Repository _repository;

        bool _confirm;

        /// <summary>
        /// Signature Fragment
        /// </summary>
        /// <returns>The instance.</returns>
        public static SignatureFragment NewInstance()
        {
            var fragment = new SignatureFragment { Arguments = new Bundle() };
            return fragment;
        }

        /// <summary>
        /// Destroy view
        /// </summary>
        public override void OnDestroyView()
        {
            _confirmButton.Click -= OnConfirmDocket;
            _repository.Dispose();
            base.OnDestroyView();
        }

        /// <summary>
        /// Create View
        /// </summary>
        /// <returns>The create view.</returns>
        /// <param name="inflater">Inflater.</param>
        /// <param name="container">Container.</param>
        /// <param name="savedInstanceState">Saved instance state.</param>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.signature_fragment, container, false);

            _repository = new Repository();

            // get the current docket id
            var profile = _repository.Profiles.First();
            _docketId = profile.CurrentDocketID;

            if (_docketId == null)
                throw new ArgumentNullException($"DocketID {_docketId} can not be null");

            BindView(view);

            return view;
        }

        public override async void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            await RefreshLocation();
        }

        async Task RefreshLocation()
        {
            if (!RequestLocationPermission())
                return;

            _location = await GetLocation();
            System.Diagnostics.Debug.WriteLine(_location?.Latitude);
        }

        /// <summary>
        /// Sets the location.
        /// </summary>
        async Task<Location> GetLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Lowest);
                return await Geolocation.GetLocationAsync(request);
            }
            catch (FeatureNotSupportedException featureNotSupportedException)
            {
                Crashes.TrackError(featureNotSupportedException);
                return null;
            }
            catch (PermissionException permissionException)
            {
                Crashes.TrackError(permissionException);
                RequestLocationPermission();
                return null;
            }
            catch (Exception exception)
            {
                Crashes.TrackError(exception);
                return null;
            }
        }

        /// <summary>
        /// Requests the location permission.
        /// </summary>
        /// <returns><c>true</c>, if location permission was requested, <c>false</c> otherwise.</returns>
        bool RequestLocationPermission()
        {
            if (Context.CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Denied)
            {
                Activity.RequestPermissions(new[] { Manifest.Permission.AccessFineLocation },
                                            LocationPermissionResult);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Requests the storage permission.
        /// </summary>
        /// <returns><c>true</c>, if storage permission was requested, <c>false</c> otherwise.</returns>
        bool RequestStoragePermission()
        {
            if (Activity.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Permission.Granted)
            {
                Activity.RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage },
                                            StoragePermissionResult);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Bind view
        /// </summary>
        /// <param name="view">View.</param>
        void BindView(View view)
        {
            _signatureView = view.FindViewById<SignaturePadView>(Resource.Id.signatureView);
            _signatureView.SignaturePromptText = Resources.GetString(Resource.String.label_sign);

            _confirmButton = view.FindViewById<Button>(Resource.Id.confirmButton);
            _confirmButton.Click += OnConfirmDocket;

            _customerNameEditText = view.FindViewById<EditText>(Resource.Id.customerNameEditText);

            HasOptionsMenu = true;

            Activity.ActionBar.SetDisplayHomeAsUpEnabled(true);

            var docket = _repository.Find<DeliveryDocket>(_docketId);
            Activity.ActionBar.Title = docket.CustomerName1;
        }


        /// <summary>
        /// Confirm docket event
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnConfirmDocket(object sender, EventArgs e)
        {
            // request permission to save signature
            if (!RequestStoragePermission())
                return;

            // confirm print docket
            var confirmMessage = Resources.GetString(Resource.String.message_confirm);
            var confirmTitle = Resources.GetString(Resource.String.button_confirm_delivery);
            var confirmDocket = await UserDialogs.Instance.ConfirmAsync(confirmMessage, confirmTitle);

            if (!confirmDocket)
                return;

            _confirmButton.Enabled = false;

            // signature saved
            var signatureSaved = UpdateDocketDetails(true);

            if (signatureSaved)
            {
                // save the signature
                await SaveSignatureToDiskAsync();

                // print docket
                var printed = await PrintDocketAsync();

                _confirmButton.Enabled = true;

                // docket is printed
                if (printed)
                {
                    NavigateToCalls();
                    return;
                }

                // docket print failed
                await Reprint();
            }

        }

        async Task Reprint()
        {
            var failedMessage = Resources.GetString(Resource.String.message_print_failed_continue);
            var failedTitle = Resources.GetString(Resource.String.title_print_failed);
            var labelYes = Resources.GetString(Resource.String.label_yes);
            var labelNo = Resources.GetString(Resource.String.label_no);

            var tryAgain = await UserDialogs.Instance.ConfirmAsync(failedMessage,
                                              failedTitle, labelYes, labelNo);

            if (tryAgain)
            {
                var printed = await PrintDocketAsync();

                if (printed)
                    NavigateToCalls();
                else
                    await Reprint();

            } else {
                NavigateToCalls();
            }

        }

        void NavigateToCalls()
        {
            Task.Run(async () =>
            {
                await ApiClient.Instance.SyncCurrentSubscriptionAsync();
            });

            _fragmentActionListener.NavigateBackToFragment(CallsFragment.TAG);
        }

        /// <summary>
        /// Prints the docket async.
        /// </summary>
        /// <returns>The docket async.</returns>
        Task<bool> PrintDocketAsync()
        {
#pragma warning disable RECS0002 // Convert anonymous method to method group
            return Task.Run(() =>
                {
                    return PrintDocket();
                });
#pragma warning restore RECS0002 // Convert anonymous method to method group
        }

        /// <summary>
        /// Save the docket
        /// </summary>
        bool UpdateDocketDetails(bool confirm)
        {
            _confirm = confirm;

            // set the signature
            var customerName = _customerNameEditText.Text;

            // validate signature
            if (_signatureView.IsBlank)
            {
                var messageSignature = Resources.GetString(Resource.String.message_missing_signature);

                Activity.RunOnUiThread(() =>
                {
                    UserDialogs.Instance.Alert(messageSignature);
                });

                return false;
            }

            byte[] signature;
            using (var stream = new MemoryStream())
            {
                var image = _signatureView.GetImage(true, true);

                image.Compress(Bitmap.CompressFormat.Png, 50, stream);
                signature = stream.ToArray();

                stream.Close();
                image.Dispose();
            }

            //var location = await GetLocation().ConfigureAwait(false);
            var longitude = _location?.Longitude;
            var latitude = _location?.Latitude;

            // open a new realm since getlocation causes context switch
            _docketId = DocketService.CommitDocket(_repository, _docketId, signature, longitude,
                                          latitude, customerName, confirm);

            // update the docket id
            var profile = _repository.Profiles.First();
            profile.CurrentDocketID = _docketId;
            _repository.SaveChanges();

            return true;
        }

        /// <summary>
        /// Save signature
        /// </summary>
        /// <returns>The signature.</returns>
        async Task SaveSignatureToDiskAsync()
        {
            var path = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures).AbsolutePath;
            var signaturePath = System.IO.Path.Combine(path, $"{_docketId}.png");

            using (var bitmap = await _signatureView.GetImageStreamAsync(SignatureImageFormat.Png,
                                                                         Color.Black, Color.White, 1f))
            {
                using (var dest = File.OpenWrite(signaturePath))
                {
                    await bitmap.CopyToAsync(dest);

                    var docket = _repository.DeliveryDockets.Find(_docketId);
                    docket.SignaturePath = signaturePath;
                    _repository.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Save instance state
        /// </summary>
        /// <param name="outState">Out state.</param>
        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            Arguments.PutString(BundleArguments.DocketId, _docketId);
        }

        /// <summary>
        /// Request permissions result
        /// </summary>
        /// <param name="requestCode">Request code.</param>
        /// <param name="permissions">Permissions.</param>
        /// <param name="grantResults">Grant results.</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {

            switch (requestCode)
            {
                case StoragePermissionResult:

                    if (grantResults.Length != 1 || grantResults[0] != (int)Permission.Granted)
                        UserDialogs.Instance.Alert(Resources.GetString(Resource.String.message_signature_permission));

                    break;

                case LocationPermissionResult:

                    if (grantResults.Length != 1 || grantResults[0] != (int)Permission.Granted)
                        UserDialogs.Instance.Alert(Resources.GetString(Resource.String.message_location_permission));

                    break;
            }


        }

        /// <summary>
        /// Create options menu
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="inflater">Inflater.</param>
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.signature_menu, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        /// <summary>
        /// Options Item Selected
        /// </summary>
        /// <returns><c>true</c>, if options item selected was oned, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.print_menu_item:

                    // check printer address has been set up
                    if (!IsPrinterSetUp())
                        return true;

                    // update docket details
                    var docketUpdated = UpdateDocketDetails(false);

                    // if docket is updated
                    if (docketUpdated)
                    {
                        // run on a seperate thread docket printing
                        Task.Run(async () =>
                        {
                            // save the signature
                            await SaveSignatureToDiskAsync().ConfigureAwait(false);

                            // print
                            var printed = PrintDocket();

                            // print failed
                            if (!printed)
                                Activity.RunOnUiThread(ShowPrintFailedMessage);
                        });
                    }

                    break;
                default:
                    _fragmentActionListener.DisplayMenu();
                    break;
            }

            return true;
        }



        /// <summary>
        /// Check the printer is set up
        /// </summary>
        /// <returns><c>true</c>, if printer was checked, <c>false</c> otherwise.</returns>
        bool IsPrinterSetUp()
        {
            // request permission to save signature
            if (!RequestStoragePermission())
                return false;

            // request location permission
            if (!RequestLocationPermission())
                return false;

            var device = _repository.MobileDevices.First();

            if (device.PrinterAddress == null)
            {
                Activity.RunOnUiThread(ShowSetupPrinterMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Print docket
        /// </summary>
        bool PrintDocket()
        {
            Activity.RunOnUiThread(ShowPrintingMessage);

            using (var repository = new Repository())
            {
                // check we have a printer set up
                var device = repository.MobileDevices.First();
                var docket = repository.DeliveryDockets.Find(_docketId);

                // get lending status
                var lendingStatus = repository.LendingStatus
                                            .Where(x => x.CustomerAccountNumber == docket.CustomerAccountNumber)
                                            .ToList();

                // get label
                var docketItems = repository.DeliveryDocketItems
                                  .Where(x => x.HasValue && x.DeliveryDocketID == docket.DocketID)
                                  .OrderBy(x => x.Description).ToList();

                var label = LabelTemplates.GetDocketLabel(docket, docketItems, lendingStatus);

                // get the printer address
                var address = device.PrinterAddress;

                if (String.IsNullOrWhiteSpace(address))
                {
                    Activity.RunOnUiThread(UserDialogs.Instance.HideLoading);
                    return false;
                }

                // print
                var result = PrinterUtils.Print(label, address, docket.SignaturePath);

                // hide loading
                Activity.RunOnUiThread(UserDialogs.Instance.HideLoading);

                return result.Status == PrintStatus.Success;
            }
        }

        /// <summary>
        /// Shows the printing message.
        /// </summary>
        void ShowPrintingMessage()
        {
            // display printing message
            var message = Resources.GetString(Resource.String.message_printing);
            UserDialogs.Instance.ShowLoading(message);
        }

        /// <summary>
        /// Shows the setup printer message.
        /// </summary>
        void ShowSetupPrinterMessage()
        {
            var message = Resources.GetString(Resource.String.message_setup_printer);
            UserDialogs.Instance.Alert(message);
        }

        /// <summary>
        /// Shows the print failed message.
        /// </summary>
        void ShowPrintFailedMessage()
        {
            var message = Resources.GetString(Resource.String.message_print_failed);
            UserDialogs.Instance.Alert(message);
        }
    }
}
