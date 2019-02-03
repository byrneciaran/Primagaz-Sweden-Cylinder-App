using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using Primagaz.Standard.Entities;
using System.Linq;
using Primagaz.Standard;

using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace Primagaz.Android
{
    public class NonDeliveryFragment:DialogFragment
    {
        public static readonly string TAG = typeof(NonDeliveryFragment).FullName;

        Action<NonDeliveryReason> _confirmAction;
        Action _cancelAction;
        List<NonDeliveryReason> _nonDeliveryReasons;
        Spinner _spinner;
        Button _confirmButton;
        Button _cancelButton;

        Repository _repository;

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <returns>The instance.</returns>
        /// <param name="confirmAction">Confirm action.</param>
        /// <param name="cancelAction">Cancel action.</param>
        public static NonDeliveryFragment NewInstance(Action<NonDeliveryReason> confirmAction, Action cancelAction)
        {
            var fragment = new NonDeliveryFragment { Arguments = new Bundle() };
            fragment._confirmAction = confirmAction;
            fragment._cancelAction = cancelAction;
            return fragment;
        }

        /// <summary>
        /// Destroy view
        /// </summary>
        public override void OnDestroyView()
        {
            _spinner.Adapter = null;

            _confirmButton.Click -= OnConfirm;
            _cancelButton.Click -= OnCancel;

            base.OnDestroyView();
        }

        /// <summary>
        /// Create view event
        /// </summary>
        /// <returns>The create view.</returns>
        /// <param name="inflater">Inflater.</param>
        /// <param name="container">Container.</param>
        /// <param name="savedInstanceState">Saved instance state.</param>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.nondelivery_dialog, container, false);

            _repository = new Repository();

            BindView(view);

            return view;
        }

        /// <summary>
        /// Bind view
        /// </summary>
        /// <param name="view">View.</param>
        void BindView(View view)
        {
            GetData();

            var names = _nonDeliveryReasons.OrderBy(x => x.SortOrder.GetValueOrDefault()).Select(x => x.Name).ToList();

            _spinner = view.FindViewById<Spinner>(Resource.Id.nonDeliverySpinner);
            _spinner.Adapter = new ArrayAdapter<string>(Activity, Resource.Layout.spinner_item, names);

            _cancelButton = view.FindViewById<Button>(Resource.Id.cancelButton);
            _cancelButton.Click += OnCancel;

            _confirmButton = view.FindViewById<Button>(Resource.Id.closeButton);
            _confirmButton.Click += OnConfirm;
        }

        /// <summary>
        /// Get data
        /// </summary>
        void GetData()
        {
            _nonDeliveryReasons = _repository.NonDeliveryReasons.OrderBy(x => x.Name).ToList();
        }

        /// <summary>
        /// Cancel event
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnCancel(object sender, EventArgs e)
        {
            _cancelAction?.Invoke();
        }


        /// <summary>
        /// Confirm event
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnConfirm(object sender, EventArgs e)
        {
            var value = _spinner.SelectedItem;

            var nonDeliveryReason = _nonDeliveryReasons.First(x => x.Name.ToLower() == value.ToString().ToLower());
            _confirmAction?.Invoke(nonDeliveryReason);
        }
    }

}
