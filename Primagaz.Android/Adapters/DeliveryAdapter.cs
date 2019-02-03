using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Primagaz.Standard.Entities;
using static Primagaz.Android.DeliveryItemViewHolder;

namespace Primagaz.Android
{
    public class DeliveryAdapter: RecyclerView.Adapter
    {
        Action<int, DeliveryItemViewHolderAction> _action;
        readonly IList<DeliveryDocketItem> _deliveryDocketItems;

        public DeliveryAdapter(IList<DeliveryDocketItem> deliveryDocketItems, Action<int, DeliveryItemViewHolderAction> action)
        {
            _deliveryDocketItems = deliveryDocketItems;
            _action = action;
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The item count.</value>
        public override int ItemCount => _deliveryDocketItems.Count;

        /// <summary>
        /// Bind ViewHolder
        /// </summary>
        /// <param name="holder">Holder.</param>
        /// <param name="position">Position.</param>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as DeliveryItemViewHolder;
            var deliveryDocketItem = _deliveryDocketItems[position];
            viewHolder.Config(deliveryDocketItem);
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
                        Inflate(Resource.Layout.delivery, parent, false);

            // create a view holder
            return new DeliveryItemViewHolder(itemView, _action);
        }
    }
}
