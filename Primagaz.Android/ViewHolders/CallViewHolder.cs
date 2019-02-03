using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Primagaz.Standard.Entities;

namespace Primagaz.Android
{
    public enum CallViewMode { Edit = 0, View = 1 };
    public enum CallViewHolderAction { NonDelivery, Delivery, Remove, Lending, LaunchMaps, LaunchPhone };

    public class CallViewHolder : RecyclerView.ViewHolder
    {
        readonly Action<int,CallViewHolderAction> _callAction;
        readonly CallViewMode _callViewMode;

        public CallViewHolder(View view, CallViewMode callViewMode, Action<int,CallViewHolderAction> callAction) : base(view)
        {
            
            _callViewMode = callViewMode;

            if (_callViewMode == CallViewMode.Edit)
            {
                view.Click += (sender, e) => callAction?.Invoke(AdapterPosition,CallViewHolderAction.Delivery);
            }

            _callAction = callAction;
        }

        /// <summary>
        /// Config the specified call.
        /// </summary>
        /// <returns>The config.</returns>
        /// <param name="call">Call.</param>
        public void Config(Call call)
        {
            using (var statusTextView = ItemView.FindViewById<TextView>(Resource.Id.statusTextView))
            {
                var backgroundResource = call.NonDelivery ? 
                                             Resource.Drawable.circle : Resource.Drawable.green_circle;

                statusTextView.SetBackgroundResource(backgroundResource);

                statusTextView.Visibility = call.Visited ? ViewStates.Visible : ViewStates.Gone;
                statusTextView.Text = call.NonDelivery ? "ND" : "V";
            }

            using (var standingOrderImageView = ItemView.FindViewById<ImageView>(Resource.Id.standingOrderImageView))
            {
                standingOrderImageView.Visibility = String.IsNullOrWhiteSpace(call.OrderType) || 
                    call.OrderType.ToLower() != "standing" ? ViewStates.Gone : ViewStates.Visible;
            }

            using (var orderNumberTextView = ItemView.FindViewById<TextView>(Resource.Id.orderNumberTextView))
            {
                orderNumberTextView.Text = call.OrderNumber;
                orderNumberTextView.Visibility = String.IsNullOrWhiteSpace(call.OrderNumber) ? ViewStates.Gone : ViewStates.Visible;
            }

            using (var accountNumberTextView = ItemView.FindViewById<TextView>(Resource.Id.accountNumberTextView))
            {
                accountNumberTextView.Text = call.CustomerAccountNumber;
            }

            using (var accountNameTextView = ItemView.FindViewById<TextView>(Resource.Id.accountNameTextView))
            {
                accountNameTextView.Text = call.CustomerName1;
            }

            using (var addressTextView = ItemView.FindViewById<TextView>(Resource.Id.addressTextView))
            {
                var address = call.Address1 + "\n" + call.PostCode + " " + call.Address4;
                addressTextView.Text = address;
            }

            using (var nonDeliveryButton = ItemView.FindViewById<Button>(Resource.Id.nonDeliveryButton))
            {
                nonDeliveryButton.Visibility = _callViewMode == CallViewMode.Edit ? ViewStates.Visible : ViewStates.Gone;
                nonDeliveryButton.Click += (sender, e) => _callAction?.Invoke(AdapterPosition,CallViewHolderAction.NonDelivery);
            }

            using (var removeCallButton = ItemView.FindViewById<Button>(Resource.Id.removeCallButton))
            {
                removeCallButton.Visibility = ViewStates.Gone;
                removeCallButton.Click += (sender, e) => _callAction?.Invoke(AdapterPosition, CallViewHolderAction.Remove);
            }

            using (var lendingButton = ItemView.FindViewById<Button>(Resource.Id.lendingButton))
            {
                lendingButton.Visibility = call.LendingStatus ? ViewStates.Visible : ViewStates.Gone;
                lendingButton.Click += (sender, e) => _callAction?.Invoke(AdapterPosition, CallViewHolderAction.Lending);
            }

            using (var onStopImageView = ItemView.FindViewById<ImageView>(Resource.Id.onStopImageView))
            {
                onStopImageView.Visibility = call.OnStop.GetValueOrDefault() ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var directionsTextView = ItemView.FindViewById<TextView>(Resource.Id.directionsTextView))
            {
                directionsTextView.Text = call.Directions1;
                directionsTextView.Visibility = String.IsNullOrWhiteSpace(call.Directions1) ? ViewStates.Gone : ViewStates.Visible;
            }

            using (var orderNumberTextView = ItemView.FindViewById<TextView>(Resource.Id.orderNumberTextView))
            {
                orderNumberTextView.Text = call.OrderNumber;
                orderNumberTextView.Visibility = String.IsNullOrWhiteSpace(call.OrderNumber) ? ViewStates.Gone : ViewStates.Visible;
            }

            using (var directionsButton = ItemView.FindViewById<Button>(Resource.Id.directionsButton))
            {
                directionsButton.Visibility = call.HasDirections ? ViewStates.Visible : ViewStates.Gone;
                directionsButton.Click += (sender, e) => _callAction?.Invoke(AdapterPosition, CallViewHolderAction.LaunchMaps);
            }

            using (var phoneButton = ItemView.FindViewById<Button>(Resource.Id.phoneButton))
            {
                phoneButton.Visibility = call.HasTelephoneNumber ? ViewStates.Visible : ViewStates.Gone;
                phoneButton.Text = call.TelephoneNumber;
                phoneButton.Click += (sender, e) => _callAction?.Invoke(AdapterPosition, CallViewHolderAction.LaunchPhone);
            }

        }
    }
}
