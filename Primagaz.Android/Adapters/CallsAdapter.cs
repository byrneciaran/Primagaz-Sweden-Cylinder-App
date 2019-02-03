using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Primagaz.Standard.Entities;
using XamarinItemTouchHelper;

namespace Primagaz.Android
{
    public class CallsAdapter: RecyclerView.Adapter
    {
        readonly Action<int, CallViewHolderAction> _callAction;
        readonly CallViewMode _callViewMode;
        List<Call> _calls;


        public CallsAdapter(List<Call> calls, CallViewMode callViewMode, Action<int, CallViewHolderAction> callAction)
        {
            _calls = calls;
            _callViewMode = callViewMode;
            _callAction = callAction;
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The item count.</value>
        public override int ItemCount => _calls.Count;

        /// <summary>
        /// Bind ViewHolder
        /// </summary>
        /// <param name="holder">Holder.</param>
        /// <param name="position">Position.</param>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as CallViewHolder;

            var call = _calls[position];
            viewHolder.Config(call);
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
                        Inflate(Resource.Layout.call, parent, false);

            // create a view holder
            return new CallViewHolder(itemView, _callViewMode, _callAction);
        }
    }
}
