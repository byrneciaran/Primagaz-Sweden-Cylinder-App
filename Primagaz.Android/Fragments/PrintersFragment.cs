
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Support.V7.Widget;
using Primagaz.Standard.Entities;
using System.Linq;
using Android.Widget;
using Primagaz.Standard;
using Acr.UserDialogs;
using LinkOS.Plugin.Abstractions;
using Android;
using Android.Content.PM;
using LinkOS.Plugin;
using System;
using Android.Bluetooth;
using Primagaz.Standard.Service;
using Android.Runtime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Primagaz.Android
{
    public class PrintersFragment : BaseFragment, IPrinterDiscovery, IDiscoveryHandler
    {
        public enum ConnectionType
        {
            Bluetooth,
            USB,
            Network
        }

        public static readonly string TAG = typeof(PrintersFragment).FullName;

        public readonly string[] PermissionsLocation = { Manifest.Permission.AccessCoarseLocation };
        public const int RequestLocationId = 0;

        List<IDiscoveredPrinter> _printers = new List<IDiscoveredPrinter>();
        PrintersAdapter _adapter;
        RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;
        RelativeLayout _placeholder;
        Button _findPrintersButton;
        ProgressBar _progressBar;

        Repository _repository;

        bool _searching;

        int _adapterPosition;

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        public static PrintersFragment NewInstance()
        {
            var fragment = new PrintersFragment { Arguments = new Bundle() };
            return fragment;
        }

        /// <summary>
        /// Destroy View
        /// </summary>
        public override void OnDestroyView()
        {
            CancelDiscovery();

            _recyclerView.SetAdapter(null);

            _findPrintersButton.Click -= OnFindPrinters;

            _repository.Dispose();

            base.OnDestroyView();
        }

        /// <summary>
        /// Create View Event
        /// </summary>
        /// <returns>The create view.</returns>
        /// <param name="inflater">Inflater.</param>
        /// <param name="container">Container.</param>
        /// <param name="savedInstanceState">Saved instance state.</param>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.printers_fragment, container, false);

            _repository = new Repository();

            BindView(view);

            return view;
        }

        /// <summary>
        /// Bind view
        /// </summary>
        /// <param name="view">View.</param>
        void BindView(View view)
        {
            _adapter = new PrintersAdapter(_printers, OnPrinterAction);

            var device = _repository.MobileDevices.FirstOrDefault();
            _adapter.DefaultAddress = device.PrinterAddress;

            _layoutManager = new LinearLayoutManager(view.Context);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_adapter);

            HasOptionsMenu = true;

            Activity.ActionBar.Show();
            Activity.ActionBar.SetDisplayHomeAsUpEnabled(false);
            Activity.ActionBar.Title = Resources.GetString(Resource.String.title_printers);

            _findPrintersButton = view.FindViewById<Button>(Resource.Id.findPrintersButton);
            _findPrintersButton.Click += OnFindPrinters;

            _progressBar = view.FindViewById<ProgressBar>(Resource.Id.progressBar);
            _progressBar.Visibility = ViewStates.Invisible;

            _placeholder = view.FindViewById<RelativeLayout>(Resource.Id.placeholder);
            TogglePlaceholderVisibility();
        }

        /// <summary>
        /// Set printer
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        /// <param name="action">Action.</param>
        void OnPrinterAction(int adapterPosition, PrinterViewHolderAction action)
        {
            _adapterPosition = adapterPosition;

            var printer = _printers[adapterPosition];

            switch (action)
            {
                case PrinterViewHolderAction.Select:
                    SetPrinter();
                    break;
                case PrinterViewHolderAction.Remove:
                    RemovePrinter();
                    break;
            }

        }

        /// <summary>
        /// Sets the printer.
        /// </summary>
        void SetPrinter()
        {
            var printer = _printers[_adapterPosition];
            SetProfilePrinter(printer.Address);
        }

        /// <summary>
        /// Set profile printer
        /// </summary>
        /// <returns><c>true</c>, if profile printer was set, <c>false</c> otherwise.</returns>
        /// <param name="address">Address.</param>
        void SetProfilePrinter(string address)
        {
            UserDialogs.Instance.ShowLoading();

            new Task(new Action(() =>
            {
                var printResult = PrinterUtils.SetDefaultPrinterSettings(address);

                Activity.RunOnUiThread(() =>
                {
                    UserDialogs.Instance.HideLoading();

                    switch (printResult.Status)
                    {
                        case PrintStatus.Success:

                            ProfileService.SetCurrentPrinter(_repository, address);

                            UserDialogs.Instance.Alert(Resources.GetString(Resource.String.message_printer_setup));
                            _fragmentActionListener.Pop();
                            break;
                        default:
                            UserDialogs.Instance.Alert(Resources.GetString(Resource.String.message_print_setup_failed));
                            break;
                    }
                });

            })).Start();

        }



        /// <summary>
        /// Remove printer
        /// </summary>
        void RemovePrinter()
        {
            ProfileService.SetCurrentPrinter(_repository, null);
            _adapter.DefaultAddress = null;

            _printers.Clear();
            _adapter.NotifyDataSetChanged();

            TogglePlaceholderVisibility();
        }

        /// <summary>
        /// Find printers click
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnFindPrinters(object sender, EventArgs e)
        {
            _searching = !_searching;

            if (_searching)
            {
                _printers.Clear();
                _adapter.NotifyDataSetChanged();
                FindBluetoothPrinters(this);
            }
            else
            {

                CancelDiscovery();
            }

        }

        /// <summary>
        /// Toggle placeholder visibility
        /// </summary>
        void TogglePlaceholderVisibility()
        {
            var printersExist = _printers.Any();
            _placeholder.Visibility = printersExist ? ViewStates.Gone : ViewStates.Visible;
            _recyclerView.Visibility = printersExist ? ViewStates.Visible : ViewStates.Gone;
        }

        /// <summary>
        /// Create options menu
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="inflater">Inflater.</param>3
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.printers_menu, menu);
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
                case Resource.Id.find_printer_menu_item:
                    ManuallyAddPrinter();
                    break;
                default:
                    _fragmentActionListener.DisplayMenu();
                    break;
            }

            return true;
        }

        /// <summary>
        /// Find Printer
        /// </summary>
        void ManuallyAddPrinter()
        {
            var config = new PromptConfig
            {
                Message = "Enter Printer Bluetooth ID",
                OnAction = OnPrinterPrompted
            };

            UserDialogs.Instance.Prompt(config);
        }

        /// <summary>
        /// Printer Prompted
        /// </summary>
        /// <param name="result">Result.</param>
        void OnPrinterPrompted(PromptResult result)
        {
            if (result.Ok)
            {
                var value = result.Text;

                var regex = String.Concat(Enumerable.Repeat("([a-fA-F0-9]{2})", 6));

                if (!Regex.IsMatch(value, regex))
                {
                    UserDialogs.Instance.Alert("Invalid Printer ID", "Printer ID");
                    return;
                }

                var address = GetMacAddress(value);
                SetProfilePrinter(address);

            }
        }

        /// <summary>
        /// Get the mac address
        /// </summary>
        /// <param name="value">Value.</param>
        string GetMacAddress(string value)
        {
            var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
            var replace = "$1:$2:$3:$4:$5:$6";
            return Regex.Replace(value, regex, replace);
        }

        /// <summary>
        /// Finds the bluetooth printers.
        /// </summary>
        /// <param name="handler">Handler.</param>
        public void FindBluetoothPrinters(IDiscoveryHandler handler)
        {
            const string permission = Manifest.Permission.AccessCoarseLocation;

            if (Activity.CheckSelfPermission(permission) == (int)Permission.Granted)
            {
                SetSearchState();
                BluetoothDiscoverer.Current.FindPrinters(Context, handler);
                return;
            }

            // Finally request permissions with the list of permissions and ID
            RequestPermissions(PermissionsLocation, RequestLocationId);
        }

        /// <summary>
        /// Request Permissions
        /// </summary>
        /// <param name="requestCode">Request code.</param>
        /// <param name="permissions">Permissions.</param>
        /// <param name="grantResults">Grant results.</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == RequestLocationId)
            {
                if (grantResults.Length != 1 || grantResults[0] != (int)Permission.Granted)
                {
                    UserDialogs.Instance.Alert(Resources.GetString(Resource.String.message_printer_location));
                    return;
                }

                FindBluetoothPrinters(this);
            }
        }



        /// <summary>
        /// Cancel discovery
        /// </summary>
        public void CancelDiscovery()
        {
            if (BluetoothAdapter.DefaultAdapter.IsDiscovering)
            {
                BluetoothAdapter.DefaultAdapter.CancelDiscovery();
                ResetSearchState();
            }
        }

        /// <summary>
        /// Reset search state
        /// </summary>
        void ResetSearchState()
        {
            if (Activity != null)
            {
                Activity.RunOnUiThread(() =>
                {
                    _progressBar.Visibility = ViewStates.Invisible;

                    var title = Resources.GetString(Resource.String.button_find_printers);
                    _findPrintersButton.Text = title;
                });
            }

        }

        /// <summary>
        /// Sets the state of the search.
        /// </summary>
        void SetSearchState()
        {
            Activity.RunOnUiThread(() =>
            {
                _progressBar.Visibility = ViewStates.Visible;

                var title = Resources.GetString(Resource.String.button_stop);
                _findPrintersButton.Text = title;
            });
        }

        /// <summary>
        /// Found printer
        /// </summary>
        /// <param name="discoveredPrinter">Discovered printer.</param>
        public void FoundPrinter(IDiscoveredPrinter discoveredPrinter)
        {
            if (!_printers.Contains(discoveredPrinter))
            {
                _printers.Add(discoveredPrinter);

                Activity.RunOnUiThread(() =>
                {
                    TogglePlaceholderVisibility();
                    _adapter.NotifyDataSetChanged();
                });
            }
        }

        /// <summary>
        /// Discovery finished
        /// </summary>
        public void DiscoveryFinished()
        {
            ResetSearchState();
        }

        /// <summary>
        /// Discovery error
        /// </summary>
        /// <param name="message">Message.</param>
        public void DiscoveryError(string message)
        {
            ResetSearchState();

            UserDialogs.Instance.Alert(Resources.GetString(Resource.String.message_printer_not_found));

            var dict = new Dictionary<String, String>
            {
                { "Printer Error Message", message }
            };

            Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Printer Not Found", dict);
        }
    }
}
