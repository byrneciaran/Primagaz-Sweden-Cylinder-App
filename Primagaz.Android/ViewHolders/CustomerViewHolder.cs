using System;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Primagaz.Standard.Entities;

namespace Primagaz.Android
{
    public class CustomerViewHolder : RecyclerView.ViewHolder
    {
        public CustomerViewHolder(View view,Action<int> selectAction) : base(view)
        {
            view.Click += (sender, e) => selectAction?.Invoke(AdapterPosition);
        }

        /// <summary>
        /// Config
        /// </summary>
        /// <returns>The config.</returns>
        /// <param name="customer">Customer.</param>
        public void Config(Customer customer)
        {
            using (var cardView = ItemView.FindViewById<CardView>(Resource.Id.cardView))
            {
                var color = customer.Selected ? Resource.Color.selectedColor : customer.HasOrder ? Resource.Color.deliveredColor : 
                                    Resource.Color.white;
                
                var backgroundColor = new Color(ContextCompat.GetColor(ItemView.Context, color));
                cardView.SetBackgroundColor(backgroundColor);
            }

            using (var accountNumberTextView = ItemView.FindViewById<TextView>(Resource.Id.accountNumberTextView))
            {
                accountNumberTextView.Text = customer.CustomerAccountNumber;
            }

            using (var customerNameTextView = ItemView.FindViewById<TextView>(Resource.Id.customerNameTextView))
            {
                customerNameTextView.Text = customer.CustomerName1;
            }

            using (var addressTextView = ItemView.FindViewById<TextView>(Resource.Id.addressTextView))
            {
                var address = customer.Address1 + "\n" + customer.PostCode + " " + customer.Address4 + "\n" + customer.TelephoneNumber;
                addressTextView.Text = address;
            }

            using (var onStopImageView = ItemView.FindViewById<ImageView>(Resource.Id.onStopImageView))
            {
                onStopImageView.Visibility = customer.OnStop ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var directionsTextView = ItemView.FindViewById<TextView>(Resource.Id.directionsTextView))
            {
                directionsTextView.Text = customer.Directions1;
                directionsTextView.Visibility = String.IsNullOrWhiteSpace(customer.Directions1) ? ViewStates.Gone : ViewStates.Visible;
            }
        }

    }
}
