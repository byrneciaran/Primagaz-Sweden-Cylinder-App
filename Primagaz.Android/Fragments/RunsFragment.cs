using Android.OS;
using Android.Views;
using Primagaz.Standard.Entities;
using Android.Support.V7.Widget;
using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using System.Linq;
using Primagaz.Standard;
using Primagaz.Standard.Service;
using Android.Widget;
using System.Collections.Generic;

namespace Primagaz.Android
{
    public class RunsFragment : BaseFragment
    {
        public static readonly string TAG = typeof(RunsFragment).FullName;

        List<Run> _runs = new List<Run>();
        RunsAdapter _adapter;
        RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;
        Run _run;
        RelativeLayout _placeholder;
        SelectTrailerFragment _selectTrailerFragment;

        Repository _repository;

        int _adapterPosition;

        string _runNumber;

        const int SaveMenuItemId = 0;
        const int Retries = 3;

        public static RunsFragment NewInstance()
        {
            var fragment = new RunsFragment { Arguments = new Bundle() };
            return fragment;
        }

        /// <summary>
        /// Destroy View
        /// </summary>
        public override void OnDestroyView()
        {
            _recyclerView.SetAdapter(null);

            if (_selectTrailerFragment != null)
                _selectTrailerFragment.Dispose();
                
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
            var view = inflater.Inflate(Resource.Layout.runs_fragment, container, false);

            _repository = new Repository();

            BindView(view, savedInstanceState);

            return view;
        }


        /// <summary>
        /// Bind view
        /// </summary>
        /// <param name="view">View.</param>
        /// <param name="savedInstanceState">Saved instance state.</param>
        void BindView(View view, Bundle savedInstanceState)
        {
           
            var profile = _repository.Profiles.First();

            _adapter = new RunsAdapter(_runs, profile.EditRuns, OnRunViewHolderAction);

            _layoutManager = new LinearLayoutManager(view.Context);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_adapter);

            HasOptionsMenu = true;

            Activity.ActionBar.Show();
            Activity.ActionBar.SetDisplayHomeAsUpEnabled(true);
            Activity.ActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu_white_24dp);

            Activity.ActionBar.Title = Resources.GetString(Resource.String.title_runs);

            // restore any persisted run
            if (savedInstanceState != null)
            {
                var runNumber = savedInstanceState.GetString(BundleArguments.RunNumber);
                _run = _repository.Runs.Find(runNumber);
            }

            _placeholder = view.FindViewById<RelativeLayout>(Resource.Id.placeholder);

            RefreshData();

        }

        /// <summary>
        /// Toggle placeholder visibility
        /// </summary>
        void TogglePlaceholderVisibility()
        {
            var runsExist = _runs.Any();
            _placeholder.Visibility = runsExist ? ViewStates.Gone : ViewStates.Visible;
            _recyclerView.Visibility = runsExist ? ViewStates.Visible : ViewStates.Gone;
        }

        /// <summary>
        /// Get data
        /// </summary>
        void RefreshData()
        {
            // get the runs
            var runs = _repository.Runs
                          .Where(x => !x.Closed)
                               .OrderBy(x => x.DeliveryDate).ToList();

            _runs.Clear();

            _runs.AddRange(runs);

            _adapter.NotifyDataSetChanged();
            TogglePlaceholderVisibility();

        }

        /// <summary>
        /// Set the run
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        void SetCurrentRun(int adapterPosition)
        {
            _adapterPosition = adapterPosition;

            _run = _runs[adapterPosition];
            _runNumber = _run.RunNumber;

            ProfileService.SetCurrentRun(_repository, _run.RunNumber);
        }

        /// <summary>
        /// Run action
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        /// <param name="action">Run action.</param>
        void OnRunViewHolderAction(int adapterPosition, RunViewHolderAction action)
        {
            SetCurrentRun(adapterPosition);

            switch (action)
            {
                case RunViewHolderAction.Close:
                    CloseRun();
                    break;
                case RunViewHolderAction.Edit:
                    NavigateToRunDetails(_run);
                    break;
                case RunViewHolderAction.Select:
                    NavigateToSelectTrailer();
                    break;
            }
        }

        /// <summary>
        /// Navigates to the trailer stock
        /// </summary>
        void NavigateToTrailerStock()
        {
            var fragment = DriverStockFragment.NewInstance(false);
            _fragmentActionListener.NavigateToFragment(fragment, DriverStockFragment.TAG);
        }

        void NavigateToSelectTrailer()
        {
            _selectTrailerFragment = SelectTrailerFragment.NewInstance(OnConfirmTrailer, OnCancelTrailer);
            _selectTrailerFragment.Show(FragmentManager, SelectTrailerFragment.TAG);
        }

        /// <summary>
        /// Confirm Trailer
        /// </summary>
        /// <param name="trailer">Trailer.</param>
        void OnConfirmTrailer(Trailer trailer)
        {
            _selectTrailerFragment.Dismiss();

            ProfileService.SetCurrentTrailer(_repository, trailer);

            NavigateToTrailerStock();
        }

        void OnCancelTrailer()
        {
            _selectTrailerFragment.Dismiss();
        }

        /// <summary>
        /// Navigate to calls
        /// </summary>
        /// <param name="mode">Mode.</param>
        void NavigateToCalls(CallsMode mode)
        {
            var fragment = CallsFragment.NewInstance(mode);
            _fragmentActionListener.NavigateToFragment(fragment, CallsFragment.TAG);
        }

        /// <summary>
        /// Close Run Event
        /// </summary>
        void CloseRun()
        {
            var runNumber = _run.RunNumber;

            // if there are any incomplete calls return
            var calls = _repository.Calls.Where(x => x.RunNumber == runNumber);

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
                        // because we are on a seperate thread we need a new realm
                        var run = _repository.Runs.Find(runNumber);
                        RunService.CloseRun(_repository, run);
                        RefreshData();

                    }
                })

            };

            UserDialogs.Instance.Confirm(confirmConfig);
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
                case Resource.Id.new_menu_item:
                    NavigateToRunDetails(null);
                    break;
                case Resource.Id.print_menu_item:
                    PrintOnStopReport();
                    break;
                default:
                    _fragmentActionListener.DisplayMenu();
                    break;
            }

            return true;
        }

        /// <summary>
        /// Print on stop
        /// </summary>
        void PrintOnStopReport()
        {
            var onStopCustomers = _repository.Customers.Where(x => x.OnStop).ToList();

            var onStopLabel = LabelTemplates.GetDenmarkOnStopLabel(onStopCustomers);

            var profile = _repository.Profiles.First();
            var device = _repository.MobileDevices.First();

            if (device.PrinterAddress == null)
            {
                var message = Resources.GetString(Resource.String.message_setup_printer);
                UserDialogs.Instance.Alert(message);
                return;
            }

            var address = device.PrinterAddress;

            var title = Resources.GetString(Resource.String.message_printing);
            UserDialogs.Instance.ShowLoading(title);

            new Task(new Action(() =>
            {
                var result = PrinterUtils.Print(onStopLabel, address);

                Activity.RunOnUiThread(() =>
                {
                    UserDialogs.Instance.HideLoading();

                    if (result.Status != PrintStatus.Success)
                    {
                        UserDialogs.Instance.Alert(result.Message);
                    }
                });

            })).Start();

        }

        /// <summary>
        /// Navigate to the run
        /// </summary>
        void NavigateToRunDetails(Run run)
        {
            var fragment = run == null ? RunFragment.NewInstance() : RunFragment.NewInstance(run.RunNumber);
            _fragmentActionListener.NavigateToFragment(fragment, RunFragment.TAG);
        }

        /// <summary>
        /// Create options menu
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="inflater">Inflater.</param>
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var profile = _repository.Profiles.First();

            if (profile.EditRuns)
            {
                inflater.Inflate(Resource.Menu.edit_runs_menu, menu);
            }
            else
            {
                inflater.Inflate(Resource.Menu.runs_menu, menu);
            }

            base.OnCreateOptionsMenu(menu, inflater);
        }

    }
}
