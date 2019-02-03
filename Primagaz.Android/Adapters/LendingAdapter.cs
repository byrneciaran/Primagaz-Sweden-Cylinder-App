using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Primagaz.Standard.Entities;

namespace Primagaz.Android
{
    public class LendingAdapter: RecyclerView.Adapter
    {
        List<LendingStatus> _lending;

        public LendingAdapter(List<LendingStatus> lending)
        {
            _lending = lending;
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The item count.</value>
        public override int ItemCount => _lending == null ? 0 : _lending.Count;

        /// <summary>
        /// Bind ViewHolder
        /// </summary>
        /// <param name="holder">Holder.</param>
        /// <param name="position">Position.</param>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as LendingViewHolder;
            var lending = _lending[position];
            viewHolder.Config(lending);
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
                        Inflate(Resource.Layout.lending, parent, false);

            // create a view holder
            return new LendingViewHolder(itemView);
        }
    }
}
