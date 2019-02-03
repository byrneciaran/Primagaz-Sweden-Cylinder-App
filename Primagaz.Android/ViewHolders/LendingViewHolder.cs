using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Primagaz.Standard.Entities;

namespace Primagaz.Android
{


    public class LendingViewHolder: RecyclerView.ViewHolder
    {
        public LendingViewHolder(View view) : base(view)
        {

        }

        /// <summary>
        /// Config the specified lending.
        /// </summary>
        /// <param name="lending">Lending.</param>
        public void Config(LendingStatus lending) 
        {
            using (var cylinderImageView = ItemView.FindViewById<ImageView>(Resource.Id.cylinderImageView))
            {
                var icon = ProductIcon.GetIcon(lending.ProductCode);

                if (icon != null)
                {
                    cylinderImageView.SetImageResource(icon.Value);
                }
            }

            using (var descriptionTextView = ItemView.FindViewById<TextView>(Resource.Id.descriptionTextView))
            {
                descriptionTextView.Text = lending.ShortDescription;
            }

            using (var quantityTextView = ItemView.FindViewById<TextView>(Resource.Id.quantityTextView))
            {
                var label = ItemView.Context.Resources.GetString(Resource.String.title_lending);
                quantityTextView.Text = String.Format("{0} {1}", lending.Quantity, label);
            }
        }

    }
}
