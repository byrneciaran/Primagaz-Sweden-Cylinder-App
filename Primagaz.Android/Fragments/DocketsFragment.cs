using Android.OS;
using Android.Views;
using Primagaz.Standard.Entities;
using Android.Support.V7.Widget;
using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using System.Linq;
using Primagaz.Standard;
using Android.Widget;
using System.Collections.Generic;

namespace Primagaz.Android
{
    public class DocketsFragment : BaseFragment
    {
        public static readonly string TAG = typeof(DocketsFragment).FullName;

        RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;
        DocketsAdapter _adapter;
        List<DeliveryDocket> _dockets;
        Repository _repository;
        RelativeLayout _placeholder;

        public static DocketsFragment NewInstance()
        {
            var fragment = new DocketsFragment { Arguments = new Bundle() };
            return fragment;
        }

        /// <summary>
        /// Destroy view
        /// </summary>
        public override void OnDestroyView()
        {
            _recyclerView.SetAdapter(null);
            _repository.Dispose();
            base.OnDestroyView();
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
            var view = inflater.Inflate(Resource.Layout.dockets_fragment, container, false);

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

            _adapter = new DocketsAdapter(_dockets, OnPrint);

            _layoutManager = new LinearLayoutManager(view.Context);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_adapter);

            HasOptionsMenu = true;

            Activity.ActionBar.Show();
            Activity.ActionBar.SetDisplayHomeAsUpEnabled(false);
            Activity.ActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu_white_24dp);

            Activity.ActionBar.Title = Resources.GetString(Resource.String.title_print_docket);

            _placeholder = view.FindViewById<RelativeLayout>(Resource.Id.placeholder);
            TogglePlaceholderVisibility();
        }

        /// <summary>
        /// Toggle placeholder visibility
        /// </summary>
        void TogglePlaceholderVisibility()
        {
            _placeholder.Visibility = ViewStates.Gone;

            //var docketsExist = _dockets.Any();
            //_placeholder.Visibility = docketsExist ? ViewStates.Gone : ViewStates.Visible;
            //_recyclerView.Visibility = docketsExist ? ViewStates.Visible : ViewStates.Gone;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        void GetData()
        {
            var profile = _repository.Profiles.First();

            var dockets = _repository.DeliveryDockets
                             .Where(x => x.Committed && x.Confirmed
                                    && x.ChildSubscriberID.ToLower() == profile.SubscriberID.ToLower())
                             .ToList();

            var unconfirmed = _repository.DeliveryDockets
                                    .Where(x => x.Committed && !x.Confirmed && 
                                           x.ChildSubscriberID.ToLower() == profile.SubscriberID.ToLower()
                                          && !dockets.Any(y => y.DocketPrefix == x.DocketPrefix))
                                    .ToList();

            dockets.AddRange(unconfirmed);

            _dockets = dockets.OrderByDescending(x => x.DateModifiedDate).ToList();
        }

        /// <summary>
        /// Print
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        void OnPrint(int adapterPosition)
        {
            var docket = _dockets[adapterPosition];
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

            var lendingStatus = _repository.LendingStatus
                         .Where(x => x.CustomerAccountNumber == docket.CustomerAccountNumber)
                          .ToList();

            var signaturePath = docket.SignaturePath;

            var docketItems = _repository.DeliveryDocketItems
                                         .Where(x => x.HasValue && x.DeliveryDocketID == docket.DocketID)
                                         .OrderBy(x => x.Description).ToList();

            var label = LabelTemplates.GetDocketLabel(docket, docketItems, lendingStatus);

            Task.Run(() =>
            {
                var result = PrinterUtils.Print(label, address, signaturePath);

                Activity.RunOnUiThread(() =>
                {
                    UserDialogs.Instance.HideLoading();

                    if (result.Status != PrintStatus.Success)
                    {
                        UserDialogs.Instance.Alert(result.Message);
                    }
                });

            });
        }

    }
}
