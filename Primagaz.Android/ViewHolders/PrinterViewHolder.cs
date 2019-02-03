using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LinkOS.Plugin.Abstractions;

namespace Primagaz.Android
{
    public enum PrinterViewHolderAction { Select,Remove };

    public class PrinterViewHolder : RecyclerView.ViewHolder
    {
        Action<int,PrinterViewHolderAction> _action;

        public PrinterViewHolder(View view, Action<int, PrinterViewHolderAction> action) : base(view)
        {
            view.Click += OnSelectPrinter;
            _action = action;

        }

        /// <summary>
        /// Config
        /// </summary>
        /// <returns>The config.</returns>
        /// <param name="printer">Printer.</param>
        public void Config(IDiscoveredPrinter printer, bool isDefault)
        {
            using (var addressTextView = ItemView.FindViewById<TextView>(Resource.Id.addressTextView))
            {
                addressTextView.Text = printer.Address;
            }

            using  (var removePrinterButton = ItemView.FindViewById<Button>(Resource.Id.removePrinterButton))
            {
                removePrinterButton.Click -= OnRemovePrinter;
                removePrinterButton.Click += OnRemovePrinter;

                removePrinterButton.Visibility = isDefault ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var defaultImageView = ItemView.FindViewById<ImageView>(Resource.Id.defaultImageView))
            {
                defaultImageView.Visibility = isDefault ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        /// <summary>
        /// Remove printer
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnRemovePrinter(object sender, EventArgs e)
        {
            _action?.Invoke(AdapterPosition, PrinterViewHolderAction.Remove);
        }


        /// <summary>
        /// Select printer
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnSelectPrinter(object sender, EventArgs e)
        {
            _action?.Invoke(AdapterPosition, PrinterViewHolderAction.Select);
        }
    }

}
