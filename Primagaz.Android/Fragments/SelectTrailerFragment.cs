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
    public class SelectTrailerFragment:DialogFragment
    {
        public static readonly string TAG = typeof(SelectTrailerFragment).FullName;

        Action<Trailer> _confirmAction;
        Action _cancelAction;
        List<Trailer> _trailers;
        List<string> _trailerNumbers;
        Spinner _spinner;
        Button _confirmButton;
        Button _cancelButton;

        Repository _repository;

        /// <summary>
        /// Select Trailer
        /// </summary>
        /// <returns>The instance.</returns>
        /// <param name="confirmAction">Confirm action.</param>
        /// <param name="cancelAction">Cancel action.</param>
        public static SelectTrailerFragment NewInstance(Action<Trailer> confirmAction, Action cancelAction)
        {
            var fragment = new SelectTrailerFragment { Arguments = new Bundle() };
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

            _repository.Dispose();

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
            var view = inflater.Inflate(Resource.Layout.trailers_dialog, container, false);

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

            _trailerNumbers = _trailers.Select(x => x.TrailerNumber).ToList();
            _trailerNumbers.Add(Resources.GetString(Resource.String.label_driver_trailer));
            _trailerNumbers.Sort();

            _spinner = view.FindViewById<Spinner>(Resource.Id.trailerSpinner);
            _spinner.Adapter = new ArrayAdapter<string>(Activity, Resource.Layout.spinner_item, _trailerNumbers);

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
            _trailers = _repository.Trailers.OrderBy(x => x.TrailerNumber).ToList();
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
            var trailerNumber = _trailerNumbers[_spinner.SelectedItemPosition];

            if (trailerNumber == Resources.GetString(Resource.String.label_driver_trailer))
                return;

            var trailer = _trailers.First(x => x.TrailerNumber == trailerNumber);
            _confirmAction?.Invoke(trailer);
        }
    }

}
