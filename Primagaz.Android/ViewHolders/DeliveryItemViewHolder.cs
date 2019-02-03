using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Primagaz.Standard.Entities;

namespace Primagaz.Android
{
    public class DeliveryItemViewHolder: RecyclerView.ViewHolder
    {
        readonly Action<int, DeliveryItemViewHolderAction> _action;
        public enum DeliveryItemViewHolderAction { Edit };

        public DeliveryItemViewHolder(View view, Action<int, DeliveryItemViewHolderAction> action) : base(view)
        {
            _action = action;

            view.Click -= OnSelectDeliveryItem;
            view.Click += OnSelectDeliveryItem;
        }

        /// <summary>
        /// Config
        /// </summary>
        /// <returns>The config.</returns>
        /// <param name="stock">Stock.</param>
        public void Config(DeliveryDocketItem deliveryItem) 
        {
            using (var cylinderImageView = ItemView.FindViewById<ImageView>(Resource.Id.cylinderImageView))
            {
                var icon = ProductIcon.GetIcon(deliveryItem.ProductCode);

                if (icon != null)
                    cylinderImageView.SetImageResource(icon.Value);
                else
                    cylinderImageView.SetImageResource(0);
            }

            using (var descriptionTextView = ItemView.FindViewById<TextView>(Resource.Id.descriptionTextView))
            {
                descriptionTextView.Text = deliveryItem.Description;
            }

            using (var fullsTextView = ItemView.FindViewById<TextView>(Resource.Id.fullsTextView))
            {
                var fullsLabel = ItemView.Context.Resources.GetString(Resource.String.label_fulls);
                fullsTextView.Text = String.Format("{0} {1}", deliveryItem.FullsDelivered, fullsLabel);

                fullsTextView.Visibility = deliveryItem.FullsDelivered > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var emptiesCollectedTextView = ItemView.FindViewById<TextView>(Resource.Id.emptiesCollectedTextView))
            {
                var emptiesCollectedLabel = ItemView.Context.Resources.GetString(Resource.String.label_empties);
                emptiesCollectedTextView.Text = String.Format("{0} {1}", deliveryItem.EmptiesCollected, emptiesCollectedLabel);

                emptiesCollectedTextView.Visibility = deliveryItem.EmptiesCollected > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var faultyFullsTextView = ItemView.FindViewById<TextView>(Resource.Id.faultyFullsTextView))
            {
                var faultyFullsLabel = ItemView.Context.Resources.GetString(Resource.String.label_faulty_fulls);
                faultyFullsTextView.Text = String.Format("{0} {1}", deliveryItem.FaultyFulls, faultyFullsLabel);

                faultyFullsTextView.Visibility = deliveryItem.FaultyFulls > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var faultyEmptiesTextView = ItemView.FindViewById<TextView>(Resource.Id.faultyEmptiesTextView))
            {
                var faultyEmptiesLabel = ItemView.Context.Resources.GetString(Resource.String.label_faulty_empties);
                faultyEmptiesTextView.Text = String.Format("{0} {1}", deliveryItem.FaultyEmpties, faultyEmptiesLabel);

                faultyEmptiesTextView.Visibility = deliveryItem.FaultyEmpties > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var fullsCollectedTextView = ItemView.FindViewById<TextView>(Resource.Id.fullsCollectedTextView))
            {
                var fullsCollectedLabel = ItemView.Context.Resources.GetString(Resource.String.label_fulls_collected);
                fullsCollectedTextView.Text = String.Format("{0} {1}", deliveryItem.FullsCollected, fullsCollectedLabel);

                fullsCollectedTextView.Visibility = deliveryItem.FullsCollected > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var emptiesDeliveredTextView = ItemView.FindViewById<TextView>(Resource.Id.emptiesDeliveredTextView))
            {
                var emptiesDeliveredLabel = ItemView.Context.Resources.GetString(Resource.String.label_empties_delivered);
                emptiesDeliveredTextView.Text = String.Format("{0} {1}", deliveryItem.EmptiesDelivered, emptiesDeliveredLabel);

                emptiesDeliveredTextView.Visibility = deliveryItem.EmptiesDelivered > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var orderedTextView = ItemView.FindViewById<TextView>(Resource.Id.orderedTextView))
            {
                var orderedLabel = ItemView.Context.Resources.GetString(Resource.String.label_ordered);
                orderedTextView.Text = String.Format("{0} {1}", deliveryItem.OrderQuantity, orderedLabel);

                orderedTextView.Visibility = deliveryItem.OrderQuantity > 0 ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        void OnSelectDeliveryItem(object sender, EventArgs e)
        {
            _action?.Invoke(AdapterPosition,DeliveryItemViewHolderAction.Edit);
        }

    }
}
