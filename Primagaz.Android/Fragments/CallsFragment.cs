using Android.OS;
using Android.Views;
using Primagaz.Standard.Entities;
using Android.Support.V7.Widget;
using System.Linq;
using Android.Widget;
using System.Threading.Tasks;
using Primagaz.Standard;
using Acr.UserDialogs;
using Primagaz.Standard.Service;
using System.Collections.Generic;
using Android.Net;
using Android.Content;
using Android.Content.PM;
using Android;
using Android.App;
using Android.Runtime;

namespace Primagaz.Android
{
    public enum CallsMode { ManageCalls = 0, ManageRun = 1 }

    public class CallsFragment : BaseFragment
    {
        public static readonly string TAG = typeof(CallsFragment).FullName;

        const int CallPermissionResult = 999;

        CallsAdapter _adapter;
        NonDeliveryFragment _nonDeliveryFragment;
        RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;
        Call _call;
        RelativeLayout _placeholder;
        CallsMode _mode;

        Repository _repository;

        List<Call> _calls = new List<Call>();


        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        /// <param name="mode">Mode.</param>
        public static CallsFragment NewInstance(CallsMode mode)
        {
            var fragment = new CallsFragment { Arguments = new Bundle() };
            fragment.Arguments.PutInt(BundleArguments.Mode, (int)mode);
            return fragment;
        }

        /// <summary>
        /// Destroy View
        /// </summary>
        public override void OnDestroyView()
        {
            _recyclerView.SetAdapter(null);
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
            var view = inflater.Inflate(Resource.Layout.calls_fragment, container, false);

            _repository = new Repository();

            BindView(view);
            return view;
        }

        /// <summary>
        /// Binds the view.
        /// </summary>
        /// <param name="view">View.</param>
        void BindView(View view)
        {
            _mode = (CallsMode)Arguments.GetInt(BundleArguments.Mode);

            var callViewMode = _mode == CallsMode.ManageRun ? CallViewMode.Edit : CallViewMode.View;

            _adapter = new CallsAdapter(_calls, callViewMode, OnCallViewHolderAction);

            _layoutManager = new LinearLayoutManager(view.Context);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_adapter);

            HasOptionsMenu = true;

            Activity.ActionBar.SetDisplayHomeAsUpEnabled(true);
            Activity.ActionBar.Title = Resources.GetString(Resource.String.title_calls);

            _placeholder = view.FindViewById<RelativeLayout>(Resource.Id.placeholder);

            RefreshData();

        }

        /// <summary>
        /// Bind
        /// </summary>
        void RefreshData()
        {
            var profile = _repository.Profiles.First();

            _calls.Clear();

            var calls = _repository.Calls
                           .Where(x => x.RunNumber == profile.CurrentRunNumber && !x.Removed)
                           .OrderBy(x => x.VisitedDate)
                           .ThenBy(x => x.Sequence)
                                .ToList();

            _calls.AddRange(calls);
            _adapter.NotifyDataSetChanged();

            TogglePlaceholderVisibility();

        }

        /// <summary>
        /// Toggle placeholder visibility
        /// </summary>
        void TogglePlaceholderVisibility()
        {
            var callsExist = _calls.Any();

            _placeholder.Visibility = callsExist ? ViewStates.Gone : ViewStates.Visible;
            _recyclerView.Visibility = callsExist ? ViewStates.Visible : ViewStates.Gone;
        }

        /// <summary>
        /// Navigate to delivery
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        void NavigateToDelivery(int adapterPosition)
        {
            _call = _calls[adapterPosition];

            if (_call.OnStop.GetValueOrDefault())
            {
                var config = new ConfirmConfig
                {
                    Message = Activity.Resources.GetString(Resource.String.message_on_stop),
                    OnAction = OnConfirmOnStopDelivery
                };

                UserDialogs.Instance.Confirm(config);
                return;
            }

            StartDelivery(_call);
        }


        /// <summary>
        /// Confirm on stop delivery
        /// </summary>
        /// <param name="confirm">If set to <c>true</c> confirm.</param>
        void OnConfirmOnStopDelivery(bool confirm)
        {
            if (confirm)
                StartDelivery(_call);
        }

        /// <summary>
        /// Starts the delivery.
        /// </summary>
        /// <param name="call">Call.</param>
        void StartDelivery(Call call)
        {
            // create and save the delivery docket
            var profile = _repository.Profiles.First();
            var subscriber = _repository.Subscribers.Find(profile.SubscriberID);

            DocketService.CreateNewDocket(_repository, profile.CurrentRunNumber, call, subscriber);

            var fragment = DeliveryFragment.NewInstance();
            _fragmentActionListener.NavigateToFragment(fragment, DeliveryFragment.TAG);
        }

        /// <summary>
        /// Navigates to lending.
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        void NavigateToLending(int adapterPosition)
        {
            var call = _calls[adapterPosition];

            var fragment = LendingFragment.NewInstance(call.CustomerAccountNumber);
            _fragmentActionListener.NavigateToFragment(fragment, LendingFragment.TAG);
        }

        /// <summary>
        /// Call action
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        /// <param name="action">Action.</param>
        void OnCallViewHolderAction(int adapterPosition, CallViewHolderAction action)
        {
            switch (action)
            {
                case CallViewHolderAction.NonDelivery:
                    NavigateToNonDelivery(adapterPosition);
                    break;
                case CallViewHolderAction.Delivery:
                    NavigateToDelivery(adapterPosition);
                    break;
                case CallViewHolderAction.Lending:
                    NavigateToLending(adapterPosition);
                    break;
                case CallViewHolderAction.LaunchMaps:
                    LaunchMaps(adapterPosition);
                    break;
                case CallViewHolderAction.LaunchPhone:
                    LaunchPhone(adapterPosition);
                    break;
            }
        }

        /// <summary>
        /// Launches the phone.
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        void LaunchPhone(int adapterPosition)
        {
            if (!RequestCallPermission())
                return;

            var confirmMessage = Resources.GetString(Resource.String.message_confirm);

            var confirmConfig = new ConfirmConfig
            {
                Message = confirmMessage,
                OnAction = (confirm) =>
                {

                    if (!confirm)
                        return;

                    var call = _calls[adapterPosition];
                    var uri = Uri.Parse(call.PhoneUri);
                    var callIntent = new Intent(Intent.ActionCall, uri);
                    StartActivity(callIntent);
                }
            };

            UserDialogs.Instance.Confirm(confirmConfig);

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
                RequestPermissions(new string[] { Manifest.Permission.CallPhone}, CallPermissionResult);

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
        /// Launch Maps
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        void LaunchMaps(int adapterPosition)
        {
            var call = _calls[adapterPosition];

            var uri = Uri.Parse(call.DirectionsUri);

            var mapIntent = new Intent(Intent.ActionView, uri);
            StartActivity(mapIntent);
        }

        /// <summary>
        /// Remove call
        /// </summary>
        /// <param name="adapterPosition">Index.</param>
        void RemoveCall(int adapterPosition)
        {
            var call = _calls[adapterPosition];

            RunService.RemoveCall(_repository, call);

            _adapter.NotifyItemRemoved(adapterPosition);
            TogglePlaceholderVisibility();
        }

        /// <summary>
        /// Navigate to Non-Delivery
        /// </summary>
        void NavigateToNonDelivery(int adapterPosition)
        {
            _call = _calls[adapterPosition];
            _nonDeliveryFragment = NonDeliveryFragment.NewInstance(OnConfirmNonDelivery, OnCancelNonDelivery);
            _nonDeliveryFragment.Show(FragmentManager, NonDeliveryFragment.TAG);
        }

        /// <summary>
        /// Confirm Close Run
        /// </summary>
        void OnConfirmNonDelivery(NonDeliveryReason nonDeliveryReason)
        {
            var adapterPosition = _calls.ToList().IndexOf(_call);
            CallService.SaveNonDelivery(_repository, _call, nonDeliveryReason);

            _nonDeliveryFragment.Dismiss();

            Task.Run(async () =>
            {
                await ApiClient.Instance.SyncCurrentSubscriptionAsync();
            });

            RefreshData();
        }

        /// <summary>
        /// Cancel Close Run
        /// </summary>
        void OnCancelNonDelivery()
        {
            _nonDeliveryFragment.Dismiss();
        }

        /// <summary>
        /// Create options menu
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="inflater">Inflater.</param>
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var resource = _mode == CallsMode.ManageRun ? Resource.Menu.manage_run_menu : Resource.Menu.manage_calls_menu;

            inflater.Inflate(resource, menu);
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
                case Resource.Id.search_menu_item:
                    ModifyCustomers();
                    break;
                case Resource.Id.print_menu_item:
                    PrintLoadAndItineryReport();
                    break;
                case Resource.Id.close_menu_item:
                    CloseRun();
                    break;
                default:
                    _fragmentActionListener.DisplayMenu();
                    break;

            }

            return true;
        }

        /// <summary>
        /// Sort the run by visited 
        /// </summary>
        void SortRun()
        {
            _calls.OrderByDescending(x => x.Visited);
            _adapter.NotifyDataSetChanged();
        }

        /// <summary>
        /// Close the run
        /// </summary>
        void CloseRun()
        {
            var profile = _repository.Profiles.First();
            var run = _repository.Runs.Find(profile.CurrentRunNumber);

            // if there are any incomplete calls return
            var calls = _repository.Calls.Where(x => x.RunNumber == run.RunNumber);

            if (calls.Any(x => !x.Visited))
            {
                var message = Resources.GetString(Resource.String.message_run_incomplete);
                UserDialogs.Instance.Alert(message);
                return;
            }

            var confirmConfig = new ConfirmConfig
            {
                Message = Resources.GetString(Resource.String.message_close_run),
                OnAction = (confirm => {
                    if (confirm)
                    {
                     
                        RunService.CloseRun(_repository, run);
                        Activity.RunOnUiThread(_fragmentActionListener.PopToRoot);
                    }
                })

            };

            UserDialogs.Instance.Confirm(confirmConfig);

        }

        /// <summary>
        /// Print the Load and Itinerary report
        /// </summary>
        void PrintLoadAndItineryReport()
        {
            var profile = _repository.Profiles.First();
            var device = _repository.MobileDevices.First();
            var subscriber = _repository.Subscribers.Find(profile.SubscriberID);

            var driverStock = _repository.DriverStock.Where(x => x.TrailerNumber == profile.CurrentTrailerNumber).ToList();

            var label = LabelTemplates.GetEndOfDayLabel(profile.CurrentRunNumber, _calls.ToList(), profile.CurrentTrailerNumber, 
                                                        profile.SubscriberID, driverStock, subscriber);

            if (device.PrinterAddress == null)
            {
                var message = Resources.GetString(Resource.String.message_setup_printer);
                UserDialogs.Instance.Alert(message);
                return;
            }

            var address = device.PrinterAddress;

            var title = Resources.GetString(Resource.String.message_printing);
            UserDialogs.Instance.ShowLoading(title);

            Task.Run(() =>
            {
                var result = PrinterUtils.Print(label, address);

                if (result.Status == PrintStatus.Success)
                {
                    //result = PrinterUtils.Print(itineraryLabel, address);

                    Activity.RunOnUiThread(() =>
                    {
                        UserDialogs.Instance.HideLoading();

                        if (result.Status != PrintStatus.Success)
                        {
                            UserDialogs.Instance.Alert(result.Message);
                        }
                    });

                }
                else
                {

                    Activity.RunOnUiThread(() =>
                    {
                        UserDialogs.Instance.HideLoading();

                        if (result.Status != PrintStatus.Success)
                            UserDialogs.Instance.Alert(result.Message);
                    });

                }

            });

        }

        /// <summary>
        /// Modify customers
        /// </summary>
        void ModifyCustomers()
        {
            var profile = _repository.Profiles.First();
            var fragment = CustomersFragment.NewInstance(profile.CurrentRunNumber);
            _fragmentActionListener.NavigateToFragment(fragment, CustomersFragment.TAG);
        }

    }
}
