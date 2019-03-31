using Android.OS;
using Android.Views;
using Primagaz.Standard.Entities;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using System.Linq;
using Android.Widget;
using Primagaz.Standard;
using Primagaz.Standard.Service;
using static Primagaz.Android.StockViewHolder;
using Acr.UserDialogs;

namespace Primagaz.Android
{
    public class DriverStockFragment : BaseFragment
    {
        public static readonly string TAG = typeof(DriverStockFragment).FullName;

        enum StockType { Fulls, Empties };
        List<Trailer> _trailers;
        Trailer _trailer;
        List<DriverStock> _driverStock = new List<DriverStock>();
        DriverStockAdapter _adapter;
        RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;
        StockItemDialog _stockItemDialog;
        DriverStock _stockItem;
        Spinner _spinner;
        Repository _repository;
        Button _resetFulls;
        Button _resetEmpties;

        int _adapterPosition;

        string _stockItemId;

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        public static DriverStockFragment NewInstance(bool isLaunchedFromSettings)
        {
            var fragment = new DriverStockFragment { Arguments = new Bundle() };
            fragment.Arguments.PutBoolean(BundleArguments.IsLaunchedFromSettings, isLaunchedFromSettings);
            return fragment;
        }

        /// <summary>
        /// Destroy View
        /// </summary>
        public override void OnDestroyView()
        {
            _recyclerView.SetAdapter(null);

            _spinner.Adapter = null;
            _spinner.ItemSelected -= OnTrailerSelected;

            _resetFulls.Click -= OnResetFulls;
            _resetEmpties.Click += OnResetEmpties;

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
            var view = inflater.Inflate(Resource.Layout.trailer_stock_fragment, container, false);

            _repository = new Repository();

            BindView(view, savedInstanceState);


            return view;
        }


        /// <summary>
        /// Bind view
        /// </summary>
        /// <param name="view">View.</param>
        void BindView(View view, Bundle savedInstanceState)
        {
            RefreshData(savedInstanceState);

            _adapter = new DriverStockAdapter(_driverStock, OnStockAction);

            _layoutManager = new LinearLayoutManager(view.Context);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_adapter);

            var names = _trailers.Select(x => x.TrailerNumber).ToList();
            var adapter = new ArrayAdapter<string>(Activity, Resource.Layout.spinner_item, names);

            _spinner = view.FindViewById<Spinner>(Resource.Id.trailersSpinner);
            _spinner.Adapter = adapter;
            _spinner.ItemSelected += OnTrailerSelected;

            // set the selected trailer
            var profile = _repository.Profiles.First();
            var trailer = names.IndexOf(profile.CurrentTrailerNumber);
            _spinner.SetSelection(trailer);

            HasOptionsMenu = true;

            var isLaunchedFromSettings = Arguments.GetBoolean(BundleArguments.IsLaunchedFromSettings);
            Activity.ActionBar.SetDisplayHomeAsUpEnabled(!isLaunchedFromSettings);

            Activity.ActionBar.Title = Resources.GetString(Resource.String.title_stock);

            _resetFulls = view.FindViewById<Button>(Resource.Id.clearFullsButton);
            _resetFulls.Click += OnResetFulls;

            _resetEmpties = view.FindViewById<Button>(Resource.Id.clearEmptiesButton);
            _resetEmpties.Click += OnResetEmpties;
        }

        /// <summary>
        /// Reset fulls
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnResetFulls(object sender, System.EventArgs e)
        {
            ResetStock(StockType.Fulls);
        }

        /// <summary>
        /// Reset empties
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnResetEmpties(object sender, System.EventArgs e)
        {
            ResetStock(StockType.Empties);
        }

        /// <summary>
        /// Reset
        /// </summary>
        /// <param name="stockType">Stock type.</param>
        void ResetStock(StockType stockType)
        {
            var titleResource = stockType == StockType.Empties ? Resource.String.button_reset_empties 
                                                      : Resource.String.button_reset_fulls;

            var title = Resources.GetString(titleResource);
            var message = Resources.GetString(Resource.String.message_confirm);

            var confirmConfig = new ConfirmConfig
            {
                Title = title,
                Message = message,
                OnAction = (confirm) => {

                    var profile = _repository.Profiles.First();

                    using (var repository = new Repository())
                    {
                        var driverStock = _repository.DriverStock.Where(x => x.TrailerNumber == profile.CurrentTrailerNumber).ToList();

                        driverStock.ForEach(driverStockItem =>
                        {
                            if (stockType == StockType.Fulls)
                            {
                                driverStockItem.Fulls = 0;
                                driverStockItem.FaultyFulls = 0;
                            }
                            else
                            {
                                driverStockItem.Empties = 0;
                                driverStockItem.FaultyEmpties = 0;
                            }
                        });

                        repository.SaveChanges();

                        Activity.RunOnUiThread(RefreshTrailer);
                    }

                }
            };

            UserDialogs.Instance.Confirm(confirmConfig);

        }


        /// <summary>
        /// Save instance state
        /// </summary>
        /// <param name="outState">Out state.</param>
        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (_stockItemId != null)
                outState.PutString(BundleArguments.StockItemId, _stockItemId);
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        void RefreshData(Bundle savedInstanceState)
        {
            _trailers = _repository.Trailers.OrderBy(x => x.TrailerNumber).ToList();

            if (savedInstanceState != null)
            {
                var stockItemId = savedInstanceState.GetString(BundleArguments.StockItemId);

                if (stockItemId != null)
                    _stockItem = _repository.Find<DriverStock>(stockItemId);
            }
        }


        /// <summary>
        /// Trailer Selected
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnTrailerSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            _trailer = _trailers[e.Position];
            ProfileService.SetCurrentTrailer(_repository, _trailer);

            RefreshTrailer();
        }

        /// <summary>
        /// Refresh trailer
        /// </summary>
        void RefreshTrailer()
        {


                // get all trailer stock
                var driverStock = _repository.DriverStock
                                     .Where(x => x.TrailerNumber == _trailer.TrailerNumber)
                                          .ToList();

                // get all products
                var products = _repository.Products.ToList();
                var orderItems = _repository.OrderItems.ToList();
                var profile = _repository.Profiles.First();

                // iterate over the products, if no stock item exists create a stock item
                products.ForEach(product =>
                {
                    var stockItem = driverStock.FirstOrDefault(x => x.ProductCode == product.ProductCode);

                    if (stockItem == null)
                    {
                        stockItem = DriverStockService.CreateTrailerStockItem(_repository, _trailer.TrailerNumber, product);
                        driverStock.Add(stockItem);

                        _repository.Add(stockItem);
                    }
                });

                // set the stock item sequence
                for (var i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    var stockItem = driverStock.FirstOrDefault(x => x.ProductCode == product.ProductCode);
                    stockItem.Sequence = product.Sequence.GetValueOrDefault();
                }

                // bind order quantities
                foreach (var stockItem in driverStock)
                {
                    stockItem.OrderQuantity = orderItems.Where(x => x.ProductCode == stockItem.ProductCode
                                                               && x.RunNumber == profile.CurrentRunNumber)
                                                                .Sum(x => x.Quantity);
                }

                _repository.SaveChanges();

                // refresh stock to sort by sequence
                driverStock = _repository.DriverStock
                         .Where(x => x.TrailerNumber == _trailer.TrailerNumber)
                         .OrderByDescending(x => x.OrderQuantity)
                         .ThenBy(x => x.Sequence)
                                           .ToList();

                // we need to create a new adapter when changing trailer
                _driverStock.Clear();
                _driverStock.AddRange(driverStock);

                Activity.RunOnUiThread(_adapter.NotifyDataSetChanged);

        }


        /// <summary>
        /// Stock action
        /// </summary>
        /// <param name="adapterPosition">Adapter position.</param>
        /// <param name="action">Action.</param>
        void OnStockAction(int adapterPosition, StockAction action)
        {
            _stockItem = _driverStock[adapterPosition];
            _stockItemId = _stockItem.Id;

            _adapterPosition = adapterPosition;

            EditStockItem(_stockItem);

        }

        /// <summary>
        /// Edit a Trailer Stock item
        /// </summary>
        /// <param name="stock">Stock item.</param>
        void EditStockItem(DriverStock stock)
        {
            var product = _repository.Products.First(x => x.ProductCode == stock.ProductCode);

            _stockItemDialog = StockItemDialog.NewTrailerStockInstance(product.ProductCode, stock.Id, OnStockItemDialogAction);
            _stockItemDialog.Show(FragmentManager, StockItemDialog.TAG);
        }

        /// <summary>
        /// On stock item dialog action
        /// </summary>
        /// <param name="action">Action.</param>
        /// <param name="product">Product.</param>
        void OnStockItemDialogAction(StockItemDialog.StockItemDialogAction action, Product product, string driverStockId)
        {
            _stockItemDialog.Dismiss();

            DriverStockService.UpdateDriverStock(_repository, driverStockId, product);
            _adapter.NotifyDataSetChanged();
        }


        /// <summary>
        /// Create options menu
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="inflater">Inflater.</param>
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var isLauchedFromSettings = Arguments.GetBoolean(BundleArguments.IsLaunchedFromSettings);

            if (!isLauchedFromSettings)
                inflater.Inflate(Resource.Menu.stock_menu, menu);

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
                    NavigateToCalls();
                    break;
                default:
                    _fragmentActionListener.DisplayMenu();
                    break;
            }

            return true;
        }

        /// <summary>
        /// Navigate to calls
        /// </summary>
        void NavigateToCalls()
        {
            var fragment = CallsFragment.NewInstance(CallsMode.ManageRun);
            _fragmentActionListener.NavigateToFragment(fragment,CallsFragment.TAG);
        }
    }
}
