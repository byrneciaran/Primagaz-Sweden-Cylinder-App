using Android.OS;
using Android.Views;
using Primagaz.Standard.Entities;
using Android.Support.V7.Widget;
using System;
using System.Linq;
using Primagaz.Standard;
using Android.Widget;
using Acr.UserDialogs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Primagaz.Android
{
    public class LendingFragment : BaseFragment
    {
        public static readonly string TAG = typeof(LendingFragment).FullName;

        List<LendingStatus> _lending;
        LendingAdapter _adapter;
        RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;
        RelativeLayout _placeholder;

        Repository _repository;

        const int SaveMenuItemId = 0;

        public static LendingFragment NewInstance(string customerAccountNumber)
        {
            var fragment = new LendingFragment { Arguments = new Bundle() };
            fragment.Arguments.PutString(BundleArguments.CustomerAccountNumber, customerAccountNumber);
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
            var view = inflater.Inflate(Resource.Layout.lending_fragment, container, false);

            _repository = new Repository();
            BindView(view);

            return view;
        }


        /// <summary>
        /// Bind view
        /// </summary>
        /// <param name="view">View.</param>
        /// <param name="savedInstanceState">Saved instance state.</param>
        void BindView(View view)
        {
            GetData();

            _adapter = new LendingAdapter(_lending);

            _layoutManager = new LinearLayoutManager(view.Context);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_adapter);

            HasOptionsMenu = true;

            Activity.ActionBar.SetDisplayHomeAsUpEnabled(true);
            Activity.ActionBar.Title = Resources.GetString(Resource.String.title_lending);

            _placeholder = view.FindViewById<RelativeLayout>(Resource.Id.placeholder);
            TogglePlaceholderVisibility();
        }

        /// <summary>
        /// Toggle placeholder visibility
        /// </summary>
        void TogglePlaceholderVisibility()
        {
            var lendingExists = _lending.Any();
            _placeholder.Visibility = lendingExists ? ViewStates.Gone : ViewStates.Visible;
            _recyclerView.Visibility = lendingExists ? ViewStates.Visible : ViewStates.Gone;
        }

        /// <summary>
        /// Get data
        /// </summary>
        void GetData()
        {
            var customerAccountNumber = Arguments.GetString(BundleArguments.CustomerAccountNumber);

            _lending = _repository.LendingStatus
                             .Where(x => x.CustomerAccountNumber == customerAccountNumber)
                                  .ToList();
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
                    PrintLending();
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
        void PrintLending()
        {
            var label = LabelTemplates.GetLendingLabel(_lending.ToList());

            var profile = _repository.Profiles.First();
            var device = _repository.MobileDevices.First();

            if (device.PrinterAddress == null)
            {
                var message = Resources.GetString(Resource.String.message_setup_printer);
                UserDialogs.Instance.Alert(message);
                return;
            }

            var title = Resources.GetString(Resource.String.message_printing);
            UserDialogs.Instance.ShowLoading(title);

            var address = device.PrinterAddress;

            new Task(new Action(() =>
            {
                var result = PrinterUtils.Print(label, address);

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
        /// Create options menu
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="inflater">Inflater.</param>
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.lending_menu, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

    }
}
