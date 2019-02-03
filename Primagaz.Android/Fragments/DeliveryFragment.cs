using Android.OS;
using Android.Views;
using Primagaz.Standard.Entities;
using Android.Support.V7.Widget;
using System.Linq;
using Android.Widget;
using Primagaz.Standard;
using Primagaz.Standard.Service;
using static Primagaz.Android.DeliveryItemViewHolder;
using Acr.UserDialogs;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Primagaz.Android
{
    public class DeliveryFragment : BaseFragment
    {
        public static readonly string TAG = typeof(DeliveryFragment).FullName;

        DeliveryAdapter _adapter;
        RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;
        StockItemDialog _stockItemDialog;
        DeliveryDocket _deliveryDocket;
        DeliveryDocketItem _docketItem;
        EditText _orderRefEditText;
        List<DeliveryDocketItem> _docketItems = new List<DeliveryDocketItem>();

        Repository _repository;

        int? _adapterPosition;

        string _docketItemId;


        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        public static DeliveryFragment NewInstance()
        {
            var fragment = new DeliveryFragment { Arguments = new Bundle() };
            return fragment;
        }

        /// <summary>
        /// Destroy view
        /// </summary>
        public override void OnDestroyView()
        {
            _recyclerView.SetAdapter(null);

            _repository.SaveChanges();
            _repository.Dispose();

            base.OnDestroyView();

        }

        /// <summary>
        /// Save instance state
        /// </summary>
        /// <param name="outState">Out state.</param>
        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (_adapterPosition.HasValue)
                outState.PutInt(BundleArguments.AdapterPosition, _adapterPosition.Value);


            if (_docketItemId != null)
                outState.PutString(BundleArguments.DocketItemId, _docketItemId);
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
            var view = inflater.Inflate(Resource.Layout.delivery_fragment, container, false);

            _repository = new Repository();

            BindView(view, savedInstanceState);

            return view;
        }

        /// <summary>
        /// Initialises the recycler view.
        /// </summary>
        /// <param name="view">View.</param>
        void BindView(View view, Bundle savedInstanceState)
        {
            _adapter = new DeliveryAdapter(_docketItems, OnDeliveryDocketItemAction);
            _layoutManager = new LinearLayoutManager(view.Context);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_adapter);

            HasOptionsMenu = true;

            Activity.ActionBar.SetDisplayHomeAsUpEnabled(true);

            _orderRefEditText = view.FindViewById<EditText>(Resource.Id.orderRefEditText);

            RefreshData();
            RestoreDocketItem(savedInstanceState);

     
        }

        /// <summary>
        /// Delivery docket item action
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        /// <param name="action">Action.</param>
        void OnDeliveryDocketItemAction(int adapterPosition, DeliveryItemViewHolderAction action)
        {
            _adapterPosition = adapterPosition;

            _docketItem = _docketItems[adapterPosition];
            _docketItemId = _docketItem.Id;

            EditDeliveryDocketItem(_docketItem);
        }

        /// <summary>
        /// Update docket item
        /// </summary>
        /// <param name="action">Action.</param>
        /// <param name="product">Product.</param>
        /// <param name="trailerStockRowGuid">Trailer stock row GUID.</param>
        void OnUpdateDocketItem(StockItemDialog.StockItemDialogAction action, Product product, string trailerStockRowGuid)
        {
            _stockItemDialog.Dismiss();

            if (action == StockItemDialog.StockItemDialogAction.Confirm)
            {
                DocketService.UpdateDocketItem(_repository, _docketItem, product);
                RefreshData();
            }
        }

        /// <summary>
        /// Edit delivery docket item
        /// </summary>
        void EditDeliveryDocketItem(DeliveryDocketItem docketItem)
        {
            PersistOrderReference();

            var product = _repository.Products.First(x => x.ProductCode == docketItem.ProductCode);

            _stockItemDialog = StockItemDialog
                .NewDeliveryItemInstance(product.ProductCode, docketItem.Id, OnUpdateDocketItem);

            _stockItemDialog.Show(FragmentManager, TAG);
        }

        /// <summary>
        /// Persist the order reference
        /// </summary>
        void PersistOrderReference()
        {
            _deliveryDocket.OrderReference = _orderRefEditText.Text;
            _repository.SaveChanges();
        }

        /// <summary>
        /// Bind
        /// </summary>
        void RefreshData()
        {

            // get the docket from saved instance state
            var profile = _repository.Profiles.First();

            _deliveryDocket = _repository.Find<DeliveryDocket>(profile.CurrentDocketID);

            if (_deliveryDocket == null)
                throw new ArgumentNullException($"Docket {profile.CurrentDocketID} does not exist.");


            _docketItems.Clear();

            var items = _repository.DeliveryDocketItems
                                      .Where(x => x.DeliveryDocketID == _deliveryDocket.DocketID)
                                             .OrderByDescending(x => x.OrderQuantity)
                                             .ThenBy(x => x.Sequence)
                                             .ToList();

            _docketItems.AddRange(items);

            _adapter.NotifyDataSetChanged();

            Activity.ActionBar.Title = _deliveryDocket.CustomerName1;
            _orderRefEditText.Text = _deliveryDocket.OrderReference;

        }

        void RestoreDocketItem(Bundle savedInstanceState)
        {
            if (savedInstanceState != null)
            {
                var adapterPosition = savedInstanceState.GetInt(BundleArguments.AdapterPosition, -1);

                if (adapterPosition != -1)
                    _adapterPosition = adapterPosition;

                _docketItemId = savedInstanceState.GetString(BundleArguments.DocketItemId);

                if (_docketItemId != null)
                    _docketItem = _repository.Find<DeliveryDocketItem>(_docketItemId);
            }
        }

        /// <summary>
        /// Create options menu
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="inflater">Inflater.</param>
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.delivery_menu, menu);
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
                case Resource.Id.forward_menu_item:
                    NavigateToSignature();
                    break;
                default:
                    _fragmentActionListener.DisplayMenu();
                    break;
            }

            return true;
        }

        /// <summary>
        /// Navigate to signature
        /// </summary>
        void NavigateToSignature()
        {
            var hasDocketItems = _repository.DeliveryDocketItems.Any(x => x.DeliveryDocketID == _deliveryDocket.DocketID && x.HasValue);

            if (!hasDocketItems)
            {
                var message = Resources.GetString(Resource.String.message_no_delivery_items);
                UserDialogs.Instance.Alert(message);
                return;
            }

            var poNumber = _orderRefEditText.Text;
            DocketService.SetDocketOrderReference(_repository, _deliveryDocket, poNumber);

            var fragment = SignatureFragment.NewInstance();
            _fragmentActionListener.NavigateToFragment(fragment, SignatureFragment.TAG);
        }

    }
}
