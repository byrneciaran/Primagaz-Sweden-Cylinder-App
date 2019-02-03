
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Primagaz.Android
{
    public class SearchFragment : BaseFragment
    {
        Button _searchButton;

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        public static SearchFragment NewInstance()
        {
            var fragment = new SearchFragment { Arguments = new Bundle() };
            return fragment;
        }

        /// <summary>
        /// Destroy View
        /// </summary>
        public override void OnDestroyView()
        {
            if (_searchButton != null) {
                _searchButton.Click -= OnSearch;
            }

            base.OnDestroyView();
        }

        /// <summary>
        /// Create View
        /// </summary>
        /// <returns>The create view.</returns>
        /// <param name="inflater">Inflater.</param>
        /// <param name="container">Container.</param>
        /// <param name="savedInstanceState">Saved instance state.</param>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.search_fragment, container, false);

            _searchButton = view.FindViewById<Button>(Resource.Id.buttonSearch);
            _searchButton.Click += OnSearch;

            SetHasOptionsMenu(true);
            Activity.ActionBar.Title = "Search Customer";

            return view;
        }


        /// <summary>
        /// Options Item Selected
        /// </summary>
        /// <returns><c>true</c>, if options item selected was oned, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            _fragmentActionListener.Pop();
            return true;
        }

        /// <summary>
        /// Create Options Menu
        /// </summary>
        /// <param name="menu">Menu.</param>
        /// <param name="inflater">Inflater.</param>
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.empty_menu, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        /// <summary>
        /// Search Click Event
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnSearch(object sender, System.EventArgs e)
        {
            //var fragment = new CustomersFragment(_run);
            //_fragmentActionListener.NavigateToFragment(fragment);
        }
    }
}
