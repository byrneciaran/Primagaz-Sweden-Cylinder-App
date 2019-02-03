using System;
using Android.Support.V7.Widget;
using Android.Views;
using Primagaz.Standard.Entities;
using System.Collections.Generic;

namespace Primagaz.Android
{
    public class RunsAdapter : RecyclerView.Adapter
    {
        readonly Action<int, RunViewHolderAction> _runAction;
        List<Run> _runs;
        readonly bool _editRunEnabled;

        public RunsAdapter(List<Run> runs, bool editRunEnabled, Action<int, RunViewHolderAction> runAction)
        {
            _runs = runs;
            _runAction = runAction;
            _editRunEnabled = editRunEnabled;
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The item count.</value>
        public override int ItemCount => _runs.Count;

        /// <summary>
        /// Bind ViewHolder
        /// </summary>
        /// <param name="holder">Holder.</param>
        /// <param name="position">Position.</param>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as RunViewHolder;
            var run = _runs[position];
            viewHolder.Config(run);
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
                        Inflate(Resource.Layout.run, parent, false);

            // create a view holder
            return new RunViewHolder(itemView, _editRunEnabled, _runAction);
        }
    }
}
