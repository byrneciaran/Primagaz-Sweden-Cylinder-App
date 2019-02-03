using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Primagaz.Standard.Entities;

namespace Primagaz.Android
{
    public class CustomersAdapter: RecyclerView.Adapter
    {
        Action<int> _selectAction;
        IList<Customer> _customers;

        public CustomersAdapter(IList<Customer> customers, Action<int> selectAction)
        {
            _customers = customers;
            _selectAction = selectAction;
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The item count.</value>
        public override int ItemCount => _customers.Count;

        /// <summary>
        /// Bind ViewHolder
        /// </summary>
        /// <param name="holder">Holder.</param>
        /// <param name="position">Position.</param>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as CustomerViewHolder;
            var customer = _customers[position];
            viewHolder.Config(customer);
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
                        Inflate(Resource.Layout.customer, parent, false);

            // create a view holder
            return new CustomerViewHolder(itemView,_selectAction);
        }
    }
}
