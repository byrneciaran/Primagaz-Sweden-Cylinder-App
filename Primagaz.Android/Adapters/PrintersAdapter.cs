using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using LinkOS.Plugin.Abstractions;

namespace Primagaz.Android
{
    public class PrintersAdapter : RecyclerView.Adapter
    {
        Action<int, PrinterViewHolderAction> _action;
        List<IDiscoveredPrinter> _printers;

        public string DefaultAddress { get; set; }

        public PrintersAdapter(List<IDiscoveredPrinter> printers, Action<int, PrinterViewHolderAction> action)
        {
            _printers = printers;
            _action = action;
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The item count.</value>
        public override int ItemCount => _printers.Count;

        /// <summary>
        /// Bind ViewHolder
        /// </summary>
        /// <param name="holder">Holder.</param>
        /// <param name="position">Position.</param>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as PrinterViewHolder;
            var printer = _printers[position];
            var isDefault = !String.IsNullOrWhiteSpace(DefaultAddress) && DefaultAddress == printer.Address;
            viewHolder.Config(printer, isDefault);
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
                        Inflate(Resource.Layout.printer, parent, false);

            // create a view holder
            return new PrinterViewHolder(itemView, _action);
        }
    }
}
