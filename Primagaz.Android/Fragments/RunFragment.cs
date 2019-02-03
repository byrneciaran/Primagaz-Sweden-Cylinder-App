using Android.OS;
using Android.Views;
using Primagaz.Standard.Entities;
using Android.Support.V7.Widget;
using Android.Widget;
using System.Linq;
using Android.Support.Design.Widget;
using Primagaz.Standard;
using Primagaz.Standard.Service;
using Android.App;
using Android.Content;
using System;
using System.Collections.Generic;

namespace Primagaz.Android
{
    public class RunFragment : BaseFragment
    {
        public static readonly string TAG = typeof(RunFragment).FullName;

        CallsAdapter _adapter;
        RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;
        Run _run;
        EditText _runNameEditText;
        RelativeLayout _placeholder;
        FloatingActionButton _addCustomerButton;

        Repository _repository;

        List<Call> _calls;

        string _runNumber;

        const int SaveMenuItemId = 0;

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        public static RunFragment NewInstance()
        {
            var fragment = new RunFragment { Arguments = new Bundle() };
            return fragment;
        }

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        /// <param name="runNumber">Run row GUID.</param>
        public static RunFragment NewInstance(string runNumber)
        {
            var fragment = new RunFragment { Arguments = new Bundle() };
            fragment.Arguments.PutString(BundleArguments.RunNumber, runNumber);
            return fragment;
        }

        /// <summary>
        /// Destroy View
        /// </summary>
        public override void OnDestroyView()
        {
            _addCustomerButton.Click -= OnAddCustomer;
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
            var view = inflater.Inflate(Resource.Layout.run_fragment, container, false);

            _repository = new Repository();

            BindView(view, savedInstanceState);

            return view;
        }

        /// <summary>
        /// Activity result
        /// </summary>
        /// <param name="requestCode">Request code.</param>
        /// <param name="resultCode">Result code.</param>
        /// <param name="data">Data.</param>
        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if ((RequestCodes)requestCode == RequestCodes.SelectCalls && resultCode == (int)Result.Ok)
                _runNumber = data.GetStringExtra(BundleArguments.RunNumber);
        }

        /// <summary>
        /// Bind view
        /// </summary>
        /// <param name="view">View.</param>
        void BindView(View view, Bundle savedInstanceState)
        {
            GetData(savedInstanceState);

   
            _adapter = new CallsAdapter(_calls, CallViewMode.View, OnCallAction);

            _layoutManager = new LinearLayoutManager(view.Context);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_adapter);

            HasOptionsMenu = true;

            Activity.ActionBar.SetDisplayHomeAsUpEnabled(true);
            Activity.ActionBar.Title = Resources.GetString(Resource.String.title_calls);

            _runNameEditText = view.FindViewById<EditText>(Resource.Id.runNameEditText);

            if (_run != null)
                _runNameEditText.Text = _run.Name;

            _placeholder = view.FindViewById<RelativeLayout>(Resource.Id.placeholder);

            _addCustomerButton = view.FindViewById<FloatingActionButton>(Resource.Id.addCustomerButton);
            _addCustomerButton.Click += OnAddCustomer;

            TogglePlaceholderVisibility();
        }

        /// <summary>
        /// Save instance state
        /// </summary>
        /// <param name="outState">Out state.</param>
        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (_runNumber != null)
                outState.PutString(BundleArguments.RunNumber, _runNumber);
        }

        /// <summary>
        /// Remove Call
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        /// <param name="action">Action.</param>
        void OnCallAction(int adapterPosition, CallViewHolderAction action)
        {
            if (action == CallViewHolderAction.Remove)
            {
                var call = _calls[adapterPosition];
                RunService.RemoveCall(_repository, call);
                //_adapter.NotifyItemChanged(adapterPosition);
            }
        }

        /// <summary>
        /// Create options menu
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="inflater">Inflater.</param>
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var title = Resources.GetString(Resource.String.menu_done);
            var saveMenuItem = menu.Add(0, SaveMenuItemId, 1, title);
            saveMenuItem.SetShowAsActionFlags(ShowAsAction.WithText | ShowAsAction.Always);

            base.OnCreateOptionsMenu(menu, inflater);
        }

        /// <summary>
        /// Add Customer Event
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnAddCustomer(object sender, System.EventArgs e)
        {
            CommitRun(false);
            NavigateToSelectCustomers();
        }

        /// <summary>
        /// Get data
        /// </summary>
        void GetData(Bundle savedInstanceState)
        {
            // try to get the run row guid from memory - potential set by activityresult
            if (_runNumber == null)
            {
                // try to get it from saved state, and finally from arguments
                _runNumber = savedInstanceState != null ?
                    savedInstanceState.GetString(BundleArguments.RunNumber) : Arguments.GetString(BundleArguments.RunNumber);
            }

            // if the row run guid doesn't exist create a new run
            _run = _runNumber == null ? RunService.CreateRun(_repository) : _repository.Find<Run>(_runNumber);

            // set the run row guid for persisting
            _runNumber = _run.RunNumber;

            _calls = _repository.Calls
                           .Where(x => x.RunNumber == _runNumber && !x.Removed)
                           .OrderBy(x => x.Sequence)
                                .ToList();
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
        /// Options Item Selected
        /// </summary>
        /// <returns><c>true</c>, if options item selected was oned, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case SaveMenuItemId:
                    CommitRun(true);
                    break;
                default:
                    _fragmentActionListener.DisplayMenu();
                    break;
            }

            return true;
        }

        /// <summary>
        /// Commit the run
        /// </summary>
        void CommitRun(bool pop)
        {
            var runName = _runNameEditText.Text;

            if (string.IsNullOrWhiteSpace(runName))
                runName = _run.RunNumber;

            RunService.UpdateRunName(_repository, _run, runName);

            if (pop)
                _fragmentActionListener.Pop();

        }

        /// <summary>
        /// Navigate to select customers
        /// </summary>
        void NavigateToSelectCustomers()
        {
            var fragment = CustomersFragment.NewInstance(_run.RunNumber);
            fragment.SetTargetFragment(this, (int)RequestCodes.SelectCalls);

            _fragmentActionListener.NavigateToFragment(fragment,CustomersFragment.TAG);
        }

    }
}
