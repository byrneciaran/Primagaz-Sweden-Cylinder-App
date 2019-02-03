using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Primagaz.Standard.Entities;

namespace Primagaz.Android
{
    public class DocketViewHolder : RecyclerView.ViewHolder
    {
        readonly Action<int> _action;

        public DocketViewHolder(View view, Action<int> action) : base(view)
        {
            _action = action;
        }

        /// <summary>
        /// Config the specified docket.
        /// </summary>
        /// <param name="docket">Docket.</param>
        public void Config(DeliveryDocket docket)
        {
            using (var accountNameTextView = ItemView.FindViewById<TextView>(Resource.Id.accountNameTextView))
            {
                accountNameTextView.Text = docket.ShortName;
            }

            using (var addressTextView = ItemView.FindViewById<TextView>(Resource.Id.addressTextView))
            {
                addressTextView.Text = docket.FormattedDateModified;
            }

            using (var docketNumberTextView = ItemView.FindViewById<TextView>(Resource.Id.docketNumberTextView))
            {
                docketNumberTextView.Text = docket.DocketID;
            }

            using (var printButton = ItemView.FindViewById<Button>(Resource.Id.printButton))
            {
                printButton.Click += OnPrint;
            }

            using (var statusImageView = ItemView.FindViewById<ImageView>(Resource.Id.statusImageView))
            {
                statusImageView.Visibility = docket.Synced ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var unconfirmedTextView = ItemView.FindViewById<TextView>(Resource.Id.unconfirmedTextView))
            {
                unconfirmedTextView.Visibility = docket.Confirmed ? ViewStates.Gone : ViewStates.Visible;
            }
        }

        void OnPrint(object sender, EventArgs e)
        {
            _action?.Invoke(AdapterPosition);
        }

    }
}
