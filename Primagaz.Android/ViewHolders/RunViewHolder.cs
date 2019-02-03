using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Primagaz.Standard.Entities;

namespace Primagaz.Android
{
    public enum RunViewHolderAction { Edit = 0, Close = 1, Select = 2 };

    public class RunViewHolder: RecyclerView.ViewHolder
    {
        readonly Action<int,RunViewHolderAction> _runAction;
        readonly bool _editRunEnabled;

        public RunViewHolder(View view, bool editRunEnabled, Action<int,RunViewHolderAction> runAction) : base(view)
        {
            view.Click -= OnSelectRun;
            view.Click += OnSelectRun;

            _runAction = runAction;

            _editRunEnabled = editRunEnabled;
        }

        /// <summary>
        /// Config
        /// </summary>
        /// <returns>The config.</returns>
        /// <param name="run">Run.</param>
        public void Config(Run run) 
        {
            using (var runNameTextView = ItemView.FindViewById<TextView>(Resource.Id.runNameTextView))
            {
                runNameTextView.Text = run.Name;
            }

            using (var runNumberTextView = ItemView.FindViewById<TextView>(Resource.Id.runNumberTextView))
            {
                runNumberTextView.Text = run.RunNumber;
            }

            using (var runDateTextView = ItemView.FindViewById<TextView>(Resource.Id.runDateTextView))
            {
                runDateTextView.Text = run.DeliveryDate?.ToString("d");
            }

            using (var editRunButton = ItemView.FindViewById<Button>(Resource.Id.editRunButton))
            {
                editRunButton.Click -= OnEditRun;
                editRunButton.Click += OnEditRun;
                editRunButton.Visibility = _editRunEnabled ? ViewStates.Visible : ViewStates.Gone;
            }

            using (var closeRunButton = ItemView.FindViewById<Button>(Resource.Id.closeRunButton))
            {
                closeRunButton.Click += OnCloseRun;
            }
        }

        void OnSelectRun(object sender, EventArgs e)
        {
            _runAction?.Invoke(AdapterPosition, RunViewHolderAction.Select);
        }

        void OnCloseRun(object sender, EventArgs e)
        {
            _runAction?.Invoke(AdapterPosition, RunViewHolderAction.Close);
        }

        void OnEditRun(object sender, EventArgs e)
        {
            _runAction?.Invoke(AdapterPosition, RunViewHolderAction.Edit);
        }
    }
}
