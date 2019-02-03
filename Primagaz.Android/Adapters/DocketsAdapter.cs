using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Primagaz.Standard.Entities;

namespace Primagaz.Android
{
    public class DocketsAdapter : RecyclerView.Adapter
    {
        readonly Action<int> _printAction;
        List<DeliveryDocket> _dockets;

        public DocketsAdapter(List<DeliveryDocket> dockets, Action<int> printAction)
        {
            _dockets = dockets;
            _printAction = printAction;
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The item count.</value>
        public override int ItemCount => _dockets == null ? 0 : _dockets.Count;

        /// <summary>
        /// Bind ViewHolder
        /// </summary>
        /// <param name="holder">Holder.</param>
        /// <param name="position">Position.</param>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as DocketViewHolder;
            var docket = _dockets[position];
            viewHolder.Config(docket);
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
                        Inflate(Resource.Layout.docket, parent, false);

            // create a view holder
            return new DocketViewHolder(itemView, _printAction);
        }
    }
}
