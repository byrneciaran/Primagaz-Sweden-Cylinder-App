using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Primagaz.Standard.Entities;

namespace Primagaz.Android
{
    public class StockViewHolder : RecyclerView.ViewHolder
    {
        public enum StockAction { Edit };

        Action<int, StockAction> _action;

        public StockViewHolder(View view, Action<int,StockAction> action) : base(view)
        {
            view.Click -= OnSelectStock;
            view.Click += OnSelectStock;

            _action = action;
        }

        /// <summary>
        /// Config
        /// </summary>
        /// <returns>The config.</returns>
        /// <param name="trailerStock">Trailer stock.</param>
        public void Config(DriverStock trailerStock)
        {
            using (var cylinderImageView = ItemView.FindViewById<ImageView>(Resource.Id.cylinderImageView))
            {
                var icon = ProductIcon.GetIcon(trailerStock.ProductCode);

                if (icon != null)
                {
                    cylinderImageView.SetImageResource(icon.Value); 
                } else {
                    cylinderImageView.SetImageResource(0);
                }
            
            }

            using (var descriptionTextView = ItemView.FindViewById<TextView>(Resource.Id.descriptionTextView))
            {
                descriptionTextView.Text = trailerStock.ShortDescription;
            }

            using (var fullsTextView = ItemView.FindViewById<TextView>(Resource.Id.fullsTextView))
            {
                var fullsLabel = ItemView.Context.Resources.GetString(Resource.String.label_fulls);
                fullsTextView.Text = String.Format("{0} {1}", trailerStock.Fulls, fullsLabel);
                fullsTextView.Visibility = trailerStock.Fulls > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var emptiesTextView = ItemView.FindViewById<TextView>(Resource.Id.emptiesTextView))
            {
                var emptiesLabel = ItemView.Context.Resources.GetString(Resource.String.label_empties);
                emptiesTextView.Text = String.Format("{0} {1}", trailerStock.Empties, emptiesLabel);
                emptiesTextView.Visibility = trailerStock.Empties > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var faultyEmptiesTextView = ItemView.FindViewById<TextView>(Resource.Id.faultyEmptiesTextView))
            {
                var faultyEmptiesLabel = ItemView.Context.Resources.GetString(Resource.String.label_faulty_empties);
                faultyEmptiesTextView.Text = String.Format("{0} {1}", trailerStock.FaultyEmpties, faultyEmptiesLabel);
                faultyEmptiesTextView.Visibility = trailerStock.FaultyEmpties > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var faultyFullsTextView = ItemView.FindViewById<TextView>(Resource.Id.faultyFullsTextView))
            {
                var faultyFullsLabel = ItemView.Context.Resources.GetString(Resource.String.label_faulty_fulls);
                faultyFullsTextView.Text = String.Format("{0} {1}", trailerStock.FaultyFulls, faultyFullsLabel);
                faultyFullsTextView.Visibility = trailerStock.FaultyFulls > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var orderedTextView = ItemView.FindViewById<TextView>(Resource.Id.orderedTextView))
            {
                var orderedLabel = ItemView.Context.Resources.GetString(Resource.String.label_ordered);
                orderedTextView.Text = String.Format("{0} {1}", trailerStock.OrderQuantity, orderedLabel);
                orderedTextView.Visibility = trailerStock.OrderQuantity > 0 ? ViewStates.Visible : ViewStates.Gone;
            }

        }

        /// <summary>
        /// Select stock
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnSelectStock(object sender, EventArgs e)
        {
            _action?.Invoke(AdapterPosition, StockAction.Edit);
        }

    }

}
