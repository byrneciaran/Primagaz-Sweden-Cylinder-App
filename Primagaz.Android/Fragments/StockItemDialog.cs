using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Primagaz.Standard;
using Primagaz.Standard.Entities;
using Android.Text;

using DialogFragment = Android.Support.V4.App.DialogFragment;
using Acr.UserDialogs;
using System.Linq;

namespace Primagaz.Android
{
    public class StockItemDialog : DialogFragment
    {
        public enum StockItemDialogAction { Confirm, Cancel };
        public enum StockItemDialogMode { Trailer, Delivery };

        public static readonly string TAG = typeof(StockItemDialog).FullName;

        Action<StockItemDialogAction, Product, string> _action;

        EditText _fullsEditText;
        EditText _emptiesEditText;
        EditText _faultyFullsEditText;
        EditText _emptiesDeliveredEditText;
        EditText _fullsCollectedEditText;
        EditText _faultyEmptiesEditText;

        RelativeLayout _fullsCollectedLayout;
        RelativeLayout _emptiesDeliveredLayout;
        RelativeLayout _faultyEmptiesLayout;

        Button _cancelButton;
        Button _confirmButton;
        StockItemDialogMode _mode;

        Repository _repository;

        Product _product;


        /// <summary>
        /// Gets or sets the faulty fulls.
        /// </summary>
        /// <value>The faulty fulls.</value>
        public int? FaultyFulls
        {
            get
            {
                if (_faultyFullsEditText == null)
                {
                    return null;
                }

                if (Int32.TryParse(_faultyFullsEditText.Text, out int value))
                {
                    return value;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the faulty empties
        /// </summary>
        /// <value>The faulty fulls.</value>
        public int? FaultyEmpties
        {
            get
            {
                if (_faultyEmptiesEditText == null)
                {
                    return null;
                }

                if (Int32.TryParse(_faultyEmptiesEditText.Text, out int value))
                {
                    return value;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the fulls.
        /// </summary>
        /// <value>The fulls.</value>
        public int? Fulls
        {
            get
            {
                if (_fullsEditText == null)
                {
                    return null;
                }

                if (Int32.TryParse(_fullsEditText.Text, out int value))
                {
                    return value;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the empties.
        /// </summary>
        /// <value>The empties.</value>
        public int? Empties
        {
            get
            {
                if (_emptiesEditText == null)
                {
                    return null;
                }

                if (Int32.TryParse(_emptiesEditText.Text, out int value))
                {
                    return value;
                }

                return null;
            }
        }

        public int? FullsCollected
        {
            get
            {
                if (_fullsCollectedEditText == null)
                {
                    return null;
                }

                if (Int32.TryParse(_fullsCollectedEditText.Text, out int value))
                {
                    return value;
                }

                return null;
            }
        }

        public int? EmptiesDelivered
        {
            get
            {
                if (_emptiesDeliveredEditText == null)
                {
                    return null;
                }

                if (Int32.TryParse(_emptiesDeliveredEditText.Text, out int value))
                {
                    return value;
                }

                return null;
            }
        }

        public static StockItemDialog NewDeliveryItemInstance(string productCode, string docketItemId,
                                                  Action<StockItemDialogAction, Product, string> action)
        {
            var fragment = new StockItemDialog { Arguments = new Bundle() };
            fragment.Arguments.PutString(BundleArguments.DocketItemId, docketItemId);
            fragment.Arguments.PutString(BundleArguments.ProductCode, productCode);
            fragment.Arguments.PutInt(BundleArguments.Mode, (int)StockItemDialogMode.Delivery);
            fragment._action = action;
            return fragment;
        }

        public static StockItemDialog NewTrailerStockInstance(string productCode, string stockItemId,
                                               Action<StockItemDialogAction, Product, string> action)
        {
            var fragment = new StockItemDialog { Arguments = new Bundle() };
            fragment.Arguments.PutString(BundleArguments.StockItemId, stockItemId);
            fragment.Arguments.PutString(BundleArguments.ProductCode, productCode);
            fragment.Arguments.PutInt(BundleArguments.Mode, (int)StockItemDialogMode.Trailer);
            fragment._action = action;
            return fragment;
        }

        /// <summary>
        /// Destroy view
        /// </summary>
        public override void OnDestroyView()
        {
            _cancelButton.Click -= OnCancel;
            _confirmButton.Click -= OnConfirm;

            _fullsEditText.TextChanged -= OnFullsTextChanged;

            _repository.Dispose();

            base.OnDestroyView();
        }

        /// <summary>
        /// Activity created 
        /// </summary>
        /// <param name="savedInstanceState">Saved instance state.</param>
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            Dialog.Window.SetSoftInputMode(SoftInput.StateVisible);
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
            var view = inflater.Inflate(Resource.Layout.stock_item_dialog, container, false);

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
            GetData();

            using (var descriptionTextView = view.FindViewById<TextView>(Resource.Id.descriptionTextView))
            {
                descriptionTextView.Text = _product.ShortDescription;
            }

            var icon = ProductIcon.GetIcon(_product.ProductCode);

            if (icon.HasValue)
            {
                using (var cylinderImageView = view.FindViewById<ImageView>(Resource.Id.cylinderImageView))
                {
                    cylinderImageView.SetImageResource(icon.Value);
                }
            }

            _fullsEditText = view.FindViewById<EditText>(Resource.Id.fullsEditText);

            if (_product.Fulls.GetValueOrDefault() > 0)
            {
                _fullsEditText.Text = _product.Fulls?.ToString();
            }

            _emptiesEditText = view.FindViewById<EditText>(Resource.Id.emptiesEditText);

            if (_product.Empties.GetValueOrDefault() > 0)
            {
                _emptiesEditText.Text = _product.Empties?.ToString();
            }

            _faultyFullsEditText = view.FindViewById<EditText>(Resource.Id.faultyFullsEditText);

            if (_product.FaultyFulls.GetValueOrDefault() > 0)
            {
                _faultyFullsEditText.Text = _product.FaultyFulls?.ToString();
            }

            _fullsCollectedEditText = view.FindViewById<EditText>(Resource.Id.fullsCollectedEditText);

            if (_product.FullsCollected.GetValueOrDefault() > 0)
            {
                _fullsCollectedEditText.Text = _product.FullsCollected?.ToString();
            }

            _emptiesDeliveredEditText = view.FindViewById<EditText>(Resource.Id.emptiesDeliveredEditText);

            if (_product.EmptiesDelivered.GetValueOrDefault() > 0)
            {
                _emptiesDeliveredEditText.Text = _product.EmptiesDelivered?.ToString();
            }

            _faultyEmptiesEditText = view.FindViewById<EditText>(Resource.Id.faultyEmptiesEditText);

            if (_product.FaultyEmpties.GetValueOrDefault() > 0)
            {
                _faultyEmptiesEditText.Text = _product.FaultyEmpties?.ToString();
            }

            // hide faulty empties if delivery
            _faultyEmptiesLayout = view.FindViewById<RelativeLayout>(Resource.Id.faultyEmptiesLayout);
            _faultyEmptiesLayout.Visibility = _mode == StockItemDialogMode.Delivery ? ViewStates.Gone : ViewStates.Visible;

            // hide fulls collected if trailer
            _fullsCollectedLayout = view.FindViewById<RelativeLayout>(Resource.Id.fullsCollectedLayout);
            _fullsCollectedLayout.Visibility = _mode == StockItemDialogMode.Trailer ? ViewStates.Gone : ViewStates.Visible;

            // hide empties delivered if trailer
            _emptiesDeliveredLayout = view.FindViewById<RelativeLayout>(Resource.Id.emptiesDeliveredLayout);
            _emptiesDeliveredLayout.Visibility = _mode == StockItemDialogMode.Trailer ? ViewStates.Gone : ViewStates.Visible;

            _cancelButton = view.FindViewById<Button>(Resource.Id.cancelButton);
            _cancelButton.Click += OnCancel;

            _confirmButton = view.FindViewById<Button>(Resource.Id.confirmButton);
            _confirmButton.Click += OnConfirm;

            // set selec to end of text
            _fullsEditText.SetSelection(_fullsEditText.Text.Length);

            SetFullsTextChangedEvent();
        }

        void SetFullsTextChangedEvent()
        {
            _fullsEditText.TextChanged -= OnFullsTextChanged;

            if (_mode == StockItemDialogMode.Delivery)
                _fullsEditText.TextChanged += OnFullsTextChanged;
        }

        /// <summary>
        /// Fulls changed event
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnFullsTextChanged(object sender, TextChangedEventArgs e)
        {
            var profile = _repository.Profiles.First();
            var stock = _repository.DriverStock.FirstOrDefault(x => x.TrailerNumber == profile.CurrentTrailerNumber 
                                                               && x.ProductCode == _product.ProductCode);

            // if we have stock greater than or equal to fulls being delivered
            // set empties
            if (stock != null && stock.Fulls >= Fulls.GetValueOrDefault())
            {
                _emptiesEditText.Text = Fulls.ToString();
                return;
            }

            var message = Resources.GetString(Resource.String.message_missing_fulls);
            UserDialogs.Instance.Alert(message);

            _fullsEditText.TextChanged -= OnFullsTextChanged;
            _fullsEditText.Text = null;

            SetFullsTextChangedEvent();

            return;
        }

        /// <summary>
        /// Get Data
        /// </summary>
        void GetData()
        {
            _mode = (StockItemDialogMode)Arguments.GetInt(BundleArguments.Mode);

            // get the product
            var productCode = Arguments.GetString(BundleArguments.ProductCode);

            _product = _repository.Find<Product>(productCode);

            switch (_mode)
            {
                case StockItemDialogMode.Trailer:
                    BindTrailerStock();
                    break;
                case StockItemDialogMode.Delivery:
                    BindDeliveryItem();
                    break;
            }

        }

        /// <summary>
        /// Bind delivery item
        /// </summary>
        void BindDeliveryItem()
        {
            // bind any trailer stock
            var docketItemId = Arguments.GetString(BundleArguments.DocketItemId);

            if (docketItemId != null)
            {
                var docketItem = _repository.Find<DeliveryDocketItem>(docketItemId);

                if (docketItem != null)
                {
                    _product.Fulls = docketItem.FullsDelivered;
                    _product.Empties = docketItem.EmptiesCollected;
                    _product.FaultyFulls = docketItem.FaultyFulls;
                    _product.EmptiesDelivered = docketItem.EmptiesDelivered;
                    _product.FullsCollected = docketItem.FullsCollected;
                    _product.OrderQuantity = docketItem.OrderQuantity;

                }
            }
        }

        /// <summary>
        /// Bind trailer stock
        /// </summary>
        void BindTrailerStock()
        {
            // bind any trailer stock
            var stockItemId = Arguments.GetString(BundleArguments.StockItemId);

            if (stockItemId != null)
            {
                var stockItem = _repository.Find<DriverStock>(stockItemId);

                if (stockItem != null)
                {
                    _product.Fulls = stockItem.Fulls;
                    _product.Empties = stockItem.Empties;
                    _product.FaultyFulls = stockItem.FaultyFulls;
                    _product.FaultyEmpties = stockItem.FaultyEmpties;
                }
            }
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnCancel(object sender, EventArgs e)
        {
            _action?.Invoke(StockItemDialogAction.Cancel, null, null);
        }

        /// <summary>
        /// Confirm
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnConfirm(object sender, EventArgs e)
        {
            _product.Fulls = Fulls;
            _product.Empties = Empties;
            _product.FaultyFulls = FaultyFulls;
            _product.FullsCollected = FullsCollected;
            _product.EmptiesDelivered = EmptiesDelivered;
            _product.FaultyEmpties = FaultyEmpties;

            var stockItemId = Arguments.GetString(BundleArguments.StockItemId);
            _action?.Invoke(StockItemDialogAction.Confirm, _product, stockItemId);
        }

    }
}
