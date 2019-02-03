using Android.OS;
using Android.Views;
using Primagaz.Standard.Entities;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Widget;
using System.Linq;
using static Android.Views.View;
using Primagaz.Standard;
using Android.Content;
using Android.App;
using System;

using Text = Android.Text;

namespace Primagaz.Android
{
    public class CustomersFragment : BaseFragment, IOnTouchListener
    {
        public static readonly string TAG = typeof(CustomersFragment).FullName;

        List<Customer> _selectedCustomers = new List<Customer>();
        List<Customer> _customers = new List<Customer>();
        CustomersAdapter _adapter;
        RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;
        EditText _filterEditText;
        Run _run;

        Repository _repository;

        CheckBox _ordersOnlyCheckBox;

        const int SaveMenuItemId = 0;


        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        /// <param name="runNumber">Run number.</param>
        public static CustomersFragment NewInstance(string runNumber)
        {
            var fragment = new CustomersFragment { Arguments = new Bundle() };
            fragment.Arguments.PutString(BundleArguments.RunNumber, runNumber);
            return fragment;
        }

        /// <summary>
        /// Destroy view
        /// </summary>
        public override void OnDestroyView()
        {
            _filterEditText.TextChanged -= OnFilterTextChanged;
            _ordersOnlyCheckBox.CheckedChange -= OnOrdersOnlyCheckChanged;

            _recyclerView.SetAdapter(null);
            _recyclerView.SetOnTouchListener(null);

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
            var view = inflater.Inflate(Resource.Layout.customers_fragment, container, false);

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
            _filterEditText = view.FindViewById<EditText>(Resource.Id.filterEditText);
            _ordersOnlyCheckBox = view.FindViewById<CheckBox>(Resource.Id.ordersOnlyCheckBox);

            GetData();

            _adapter = new CustomersAdapter(_customers, OnSelectCustomer);

            _layoutManager = new LinearLayoutManager(view.Context);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_adapter);
            _recyclerView.SetOnTouchListener(this);

            HasOptionsMenu = true;

            Activity.ActionBar.Show();
            Activity.ActionBar.SetDisplayHomeAsUpEnabled(true);
            Activity.ActionBar.Title = Resources.GetString(Resource.String.title_customers);

            _filterEditText.TextChanged += OnFilterTextChanged;
            _ordersOnlyCheckBox.CheckedChange += OnOrdersOnlyCheckChanged;
        }


        void OnOrdersOnlyCheckChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            GetData();
            _adapter.NotifyDataSetChanged();
        }

        /// <summary>
        /// Recycler view touch event
        /// </summary>
        /// <returns><c>true</c>, if touch was oned, <c>false</c> otherwise.</returns>
        /// <param name="v">V.</param>
        /// <param name="e">E.</param>
        public bool OnTouch(View v, MotionEvent e)
        {
            _fragmentActionListener.HideSoftInput();
            return false;
        }

        /// <summary>
        /// Filter text changed event
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnFilterTextChanged(object sender, Text.TextChangedEventArgs e)
        {
            GetData();
            _adapter.NotifyDataSetChanged();
        }

        void GetData()
        {
            var query = _filterEditText.Text;

            var runNumber = Arguments.GetString(BundleArguments.RunNumber);

            _run = _repository.Find<Run>(runNumber);

            // get all customers
            var allCustomers = _repository.Customers
                                     .OrderBy(x => x.CustomerName1)
                                     .ToList();

            // filter the customers

            var filteredCustomers = String.IsNullOrWhiteSpace(query) ? allCustomers : allCustomers
                                          .Where(x => Filter(x, query))
                                          .OrderBy(x => x.CustomerName1).ToList();

            // flag the customer if it's in the selected list
            foreach (var call in filteredCustomers)
            {
                var customer = _selectedCustomers
                    .FirstOrDefault(x => x.CustomerAccountNumber == call.CustomerAccountNumber);

                if (customer != null)
                {
                    customer.Selected = true;
                }
            }

            // get all orders
            var orders = _repository.Orders.ToList().Select(x => x.CustomerAccountNumber).ToList();

            if (_ordersOnlyCheckBox.Checked)
            {
                filteredCustomers = filteredCustomers.Where(x => orders.Contains(x.CustomerAccountNumber))
                                                     .OrderBy(x => x.CustomerName1).ToList();
            }

            // reset current list
            _customers.Clear();
            _customers.AddRange(filteredCustomers);
        }

        /// <summary>
        /// Filter customers
        /// </summary>
        /// <returns>The filter.</returns>
        /// <param name="customer">Customer.</param>
        /// <param name="query">Query.</param>
        bool Filter(Customer customer, string query)
        {
            if (customer.CustomerName1 != null &&
                customer.CustomerName1.ToLower().Contains(query.ToLower()))
            {
                return true;
            }

            if (customer.CustomerAccountNumber != null &&
                customer.CustomerAccountNumber.ToLower().Contains(query.ToLower()))
            {
                return true;
            }

            if (customer.Address4 != null &&
                customer.Address4.ToLower().Contains(query.ToLower()))
            {
                return true;
            }

            if (customer.PostCode != null &&
                customer.PostCode.ToLower().Contains(query.ToLower()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Select Row
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        void OnSelectCustomer(int adapterPosition)
        {
            var customer = _customers[adapterPosition];

            if (customer == null)
                return;

            customer.Selected = !customer.Selected;

            var calls = _repository.Calls.ToList();

            var call = calls.FirstOrDefault(x => x.RunNumber == _run.RunNumber &&
                                            x.CustomerAccountNumber == customer.CustomerAccountNumber);

            // add the calll
            if (customer.Selected && call == null)
            {
                var id = String.Format("{0}{1}", _run.RunNumber, customer.CustomerAccountNumber);
                var lendingStatus = customer.LendingStatus.GetValueOrDefault();

                var firstCall = calls.OrderBy(x => x.Sequence).FirstOrDefault();
                var sequence = firstCall != null ? firstCall.Sequence - 1 : 1;

                var newCall = new Call
                {
                    Id = id,
                    CustomerAccountNumber = customer.CustomerAccountNumber,
                    ShortName = customer.ShortName,
                    RunNumber = _run.RunNumber,
                    CustomerName1 = customer.CustomerName1,
                    Address1 = customer.Address1,
                    Address2 = customer.Address2,
                    Address3 = customer.Address3,
                    Address4 = customer.Address4,
                    PostCode = customer.PostCode,
                    OrderNumber = customer.OrderNumber,
                    TelephoneNumber = customer.TelephoneNumber,
                    Directions1 = customer.Directions1,
                    Directions2 = customer.Directions2,
                    Longitude = customer.Longitude,
                    Latitude = customer.Latitude,
                    OnStop = customer.OnStop,
                    Removed = false,
                    Sequence = sequence,
                    LendingStatus = lendingStatus,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                newCall.SetVisited(false);
                _repository.Add(newCall);
            }
            else if (!customer.Selected && call != null)
            {
                _repository.Remove(call);
            }

            _repository.SaveChanges();

            _adapter.NotifyItemChanged(adapterPosition);
        }

        /// <summary>
        /// Create options menu
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="inflater">Inflater.</param>
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var done = Resources.GetString(Resource.String.menu_done);
            var saveMenuItem = menu.Add(0, SaveMenuItemId, 1, done);
            saveMenuItem.SetShowAsActionFlags(ShowAsAction.WithText | ShowAsAction.Always);

            base.OnCreateOptionsMenu(menu, inflater);
        }

        /// <summary>
        /// Options Item Selected
        /// </summary>
        /// <returns><c>true</c>, if options item selected was oned, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            UpdateTargetFragment();

            switch (item.ItemId)
            {
                case SaveMenuItemId:
                    UpdateTargetFragment();
                    _fragmentActionListener.Pop();
                    break;
                default:
                    _fragmentActionListener.DisplayMenu();
                    break;
            }

            return true;
        }


        /// <summary>
        /// Update the target fragment
        /// </summary>
        void UpdateTargetFragment()
        {
            if (_run != null && TargetFragment != null)
            {
                var intent = new Intent();
                intent.PutExtra(BundleArguments.RunNumber, _run.RunNumber);
                TargetFragment.OnActivityResult((int)RequestCodes.SelectCalls, (int)Result.Ok, intent);
            }
        }
    }
}
