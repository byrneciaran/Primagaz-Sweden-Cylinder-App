using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Primagaz.Standard.Entities;
using static Primagaz.Android.StockViewHolder;

namespace Primagaz.Android
{
    public class DriverStockAdapter: RecyclerView.Adapter
    {
        readonly Action<int, StockAction> _action;
        List<DriverStock> _stock;

        public DriverStockAdapter(List<DriverStock> stock, Action<int, StockAction> action)
        {
            _stock = stock;
            _action = action;
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The item count.</value>
        public override int ItemCount => _stock == null ? 0 : _stock.Count;

        /// <summary>
        /// Bind ViewHolder
        /// </summary>
        /// <param name="holder">Holder.</param>
        /// <param name="position">Position.</param>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as StockViewHolder;
            var stock = _stock[position];
            viewHolder.Config(stock);
        }

        /// <summary>
        /// Create ViewHolder
        /// </summary>
        /// <returns>The create view holder.</returns>
        /// <param name="parent">Parent.</param>
        /// <param name="viewType">View type.</param>
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).
                        Inflate(Resource.Layout.stock, parent, false);

            // create a view holder
            return new StockViewHolder(itemView, _action);
        }
    }
}
