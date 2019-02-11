using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Acr.UserDialogs;
using Primagaz.Standard;
using Primagaz.Android.Sync;
using Android.Content;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using Xamarin.Essentials;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Android.Support.V4.App;
using Android.Accounts;
using Amazon;
using System.Collections.Generic;
using Android.Graphics;
using System.Linq;
using Android.Runtime;

using Manifest = Android.Manifest;
using ScreenOrientation = Android.Content.PM.ScreenOrientation;
using Fragment = Android.Support.V4.App.Fragment;

namespace Primagaz.Android
{
    [Activity(Label = "@string/app_name",
    Theme = "@style/AppTheme",
    RoundIcon = "@mipmap/ic_round_launcher",
    Icon = "@mipmap/ic_launcher",
    WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustResize,
    AlwaysRetainTaskState = true,
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleInstance,
    ConfigurationChanges = ConfigChanges.Locale,
    ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FragmentActivity, IFragmentActionListener
    {
        NavigationView _navigationView;
        DrawerLayout _drawerLayout;

        const string AppSecret = "681514df-a4c7-4c62-a7be-0829cdbb5c84";
        const int RequestPermissionsResult = 1000;

        // Poll Frequency for Sync Adapter is 3600 Seconds = 1 Hour
        const int SyncFrequencySeconds = 3600;

        Account _account;

        readonly string[] _permissions = {
                Manifest.Permission.CallPhone,
                Manifest.Permission.AccessFineLocation,
                Manifest.Permission.WriteExternalStorage };

        /// <summary>
        /// Create event
        /// </summary>
        /// <param name="savedInstanceState">Saved instance state.</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetupAWS();

            AppCenter.Start(AppSecret, typeof(Analytics), typeof(Crashes));

            RegisterServiceLocator();

            UserDialogs.Init(this);
            VersionTracking.Track();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.main_activity);

            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            _navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            _navigationView.NavigationItemSelected += OnNavigationItemSelected;

            // Set the action bar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);

            // initialise login fragment
            var loginFragment = LoginFragment.NewInstance();

            SupportFragmentManager.BeginTransaction()
                            .Add(Resource.Id.fragment_container, loginFragment)
                            .Commit();

            // Enable periodic sync
            EnablePeriodicSync();

            // Check permissions
            CheckPermissions();

        }

        /// <summary>
        /// Checks the permissions.
        /// </summary>
        void CheckPermissions()
        {
            var permissionDenied = false;

            foreach (var permission in _permissions)
            {
                permissionDenied = CheckSelfPermission(permission) != Permission.Granted;

                if (permissionDenied)
                    break;
            }

            if (permissionDenied)
                RequestPermission();
        }

        /// <summary>
        /// Request Device IMEI Permission
        /// </summary>
        void RequestPermission()
        {
            var showRationale = false;

            foreach (var permission in _permissions)
            {
                showRationale = ShouldShowRequestPermissionRationale(permission);

                if (showRationale)
                    break;
            }

            // Provide an additional rationale to the user if the permission was not granted
            // and the user would benefit from additional context for the use of the permission.
            // For example if the user has previously denied the permission.
            if (showRationale)
                ShowRequestPermissions();
            else
                RequestPermissions(_permissions, RequestPermissionsResult);
        }

        /// <summary>
        /// Shows the request device imei permission alert.
        /// </summary>
        void ShowRequestPermissions()
        {

            var message = Resources.GetString(Resource.String.message_permission_primagaz);

            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Permission Request");
            builder.SetMessage(message);
            builder.SetCancelable(false);
            builder.SetPositiveButton("Ok", (sender, e) =>
            {
                RequestPermissions(_permissions, RequestPermissionsResult);
            });

            builder.Show();
        }

        /// <summary>
        /// Request permissions result
        /// </summary>
        /// <param name="requestCode">Request code.</param>
        /// <param name="permissions">Permissions.</param>
        /// <param name="grantResults">Grant results.</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == RequestPermissionsResult)
            {
                if (!grantResults.Any() || grantResults.First() == Permission.Denied)
                    ShowRequestPermissions();
            }
        }

        /// <summary>
        /// Setup AWS
        /// </summary>
        void SetupAWS()
        {
            AWSConfigs.AWSRegion = "eu-west-1";

            var loggingConfig = AWSConfigs.LoggingConfig;
            loggingConfig.LogMetrics = true;
            loggingConfig.LogResponses = ResponseLoggingOption.Always;
            loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
            loggingConfig.LogTo = LoggingOptions.SystemDiagnostics;

        }

        /// <summary>
        /// Hides soft input when focus moves
        /// </summary>
        /// <returns><c>true</c>, if touch event was dispatched, <c>false</c> otherwise.</returns>
        /// <param name="ev">Ev.</param>
        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            if (ev.Action == MotionEventActions.Down)
            {
                var view = CurrentFocus;

                if (view is EditText)
                {
                    var rect = new Rect();
                    view.GetGlobalVisibleRect(rect);

                    if (!rect.Contains((int)ev.RawX,(int)ev.RawY))
                    {
                        view.ClearFocus();

                        var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                        inputManager.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
                    }
                }
            }

            return base.DispatchTouchEvent(ev);
        }


        /// <summary>
        /// Enables the periodic sync.
        /// </summary>
        void EnablePeriodicSync()
        {
            _account = AccountUtils.CreateSyncAccount(this);

            // set whether the account is syncable
            ContentResolver.SetIsSyncable(_account, AccountUtils.SyncAuthority, 1);

            // set to sync whenever it receives a network tickle
            ContentResolver.SetSyncAutomatically(_account, AccountUtils.SyncAuthority, true);

            // set to sync every x seconds
            ContentResolver.RemovePeriodicSync(_account, AccountUtils.SyncAuthority, Bundle.Empty);
            ContentResolver.AddPeriodicSync(_account, AccountUtils.SyncAuthority, Bundle.Empty, SyncFrequencySeconds);
        }

        /// <summary>
        /// Register the DI Service Locator
        /// </summary>
        static void RegisterServiceLocator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DeviceManager>().As<IDeviceManager>();

            var container = builder.Build();

            var csl = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);
        }

        /// <summary>
        /// Navigation Item Selected
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnNavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            _drawerLayout.CloseDrawers();

            switch (e.MenuItem.ItemId)
            {
                case Resource.Id.edit_trailer_menu_item:
                    NavigateToTrailerStock();
                    break;
                case Resource.Id.printer_menu_item:
                    NavigateToPrinter();
                    break;
                case Resource.Id.logout_menu_item:
                    NavigateToLogout();
                    break;
                case Resource.Id.sync_menu_item:
                    NavigateToSync();
                    break;
                case Resource.Id.dockets_menu_item:
                    NavigateToDockets();
                    break;
                case Resource.Id.support_menu_item:
                    NavigateToSupport();
                    break;
            }


        }

        /// <summary>
        /// Navigates to logout.
        /// </summary>
        void NavigateToLogout()
        {
            PopToRoot();

            _drawerLayout.CloseDrawers();

            var fragment = LoginFragment.NewInstance();
            ReplaceFragment(fragment, LoginFragment.TAG);
        }

        /// <summary>
        /// Navigate to Stock
        /// </summary>
        void NavigateToTrailerStock()
        {

            if (SupportFragmentManager.BackStackEntryCount != 0)
            {
                var count = SupportFragmentManager.BackStackEntryCount - 1;
                var entry = SupportFragmentManager.GetBackStackEntryAt(count);

                // prevent jumping into stock screen from stock screen
                if (entry.Name == DriverStockFragment.TAG)
                    return;

            }

            var fragment = DriverStockFragment.NewInstance(true);
            NavigateToFragment(fragment, DriverStockFragment.TAG);
        }

        void NavigateToDockets()
        {
            var fragment = DocketsFragment.NewInstance();
            NavigateToFragment(fragment, DocketsFragment.TAG);
        }

        /// <summary>
        /// Navigate to Printer
        /// </summary>
        void NavigateToPrinter()
        {
            var fragment = PrintersFragment.NewInstance();
            NavigateToFragment(fragment, PrintersFragment.TAG);
        }

        /// <summary>
        /// Display Menu
        /// </summary>
        public void DisplayMenu()
        {
            _drawerLayout.OpenDrawer(GravityCompat.Start);
        }

        /// <summary>
        /// Hide the soft input
        /// </summary>
        public void HideSoftInput()
        {
            var inputManager = (InputMethodManager)GetSystemService(InputMethodService);

            if (inputManager != null && CurrentFocus != null)
                inputManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
        }

        public void ShowSoftInput(View view)
        {
            var inputManager = (InputMethodManager)GetSystemService(InputMethodService);

            if (inputManager != null && CurrentFocus != null)
                inputManager.ShowSoftInput(view, ShowFlags.Implicit);

        }

        public override void OnBackPressed()
        {
            if (SupportFragmentManager.BackStackEntryCount > 0)
                base.OnBackPressed();
        }


        /// <summary>
        /// Navigate to fragment
        /// </summary>
        /// <param name="fragment">Fragment.</param>
        public void NavigateToFragment(BaseFragment fragment, string name)
        {
            HideSoftInput();

            _drawerLayout.CloseDrawers();

            SupportFragmentManager
                 .BeginTransaction()
                 .Replace(Resource.Id.fragment_container, fragment, name)
                 .AddToBackStack(name)
                 .CommitAllowingStateLoss();
        }

        /// <summary>
        /// Pop fragment
        /// </summary>
        public void Pop()
        {
            RunOnUiThread(() =>
            {
                _drawerLayout.CloseDrawers();
                HideSoftInput();

                try
                {
                    SupportFragmentManager.PopBackStack();
                }
                catch (Java.Lang.RuntimeException exception)
                {
                    Crashes.TrackError(exception);
                }
            });
        }

        /// <summary>
        /// Pops to root.
        /// </summary>
        public void PopToRoot()
        {
            var count = SupportFragmentManager.BackStackEntryCount;

            // if we are already at start return
            if (count == 0)
                return;

            RunOnUiThread(() =>
            {
                _drawerLayout.CloseDrawers();
                HideSoftInput();

                try
                {
                    SupportFragmentManager.PopBackStackImmediate(0,
                    (int)PopBackStackFlags.Inclusive);
                }
                catch (Java.Lang.RuntimeException exception)
                {
                    Crashes.TrackError(exception);
                }

            });

        }

        /// <summary>
        /// Pop to a specific fragment name
        /// </summary>
        /// <param name="name">Name.</param>
        public void NavigateBackToFragment(string name)
        {
            // get out fragment we want to navigate to
            var fragment = SupportFragmentManager.FindFragmentByTag(name);

            // pop everything up to our fragment
            RunOnUiThread(() =>
            {
                _drawerLayout.CloseDrawers();
                HideSoftInput();

                try
                {
                    SupportFragmentManager.PopBackStackImmediate(name,
                             (int)PopBackStackFlags.None);


                }
                catch (Java.Lang.RuntimeException exception)
                {
                    var dict = new Dictionary<string, string>
                    {
                        { "Fragment Name", name }
                    };

                    Crashes.TrackError(exception, dict);
                }
            });

        }

        public void NavigateToSupport()
        {
            _drawerLayout.CloseDrawers();

            var fragment = SupportFragment.NewInstance();
            NavigateToFragment(fragment, SyncFragment.TAG);
        }

        /// <summary>
        /// Sync
        /// </summary>
        public void NavigateToSync()
        {
            _drawerLayout.CloseDrawers();

            var fragment = SyncFragment.NewInstance();
            NavigateToFragment(fragment, SyncFragment.TAG);
        }

        public void ToggleDrawer()
        {
            _drawerLayout.CloseDrawers();
        }

        /// <summary>
        /// Sets the display username
        /// </summary>
        /// <param name="username">Username.</param>
        public void SetDisplayUsername(string username)
        {
            var headerView = _navigationView.GetHeaderView(0);

            using (var usernameText = headerView.FindViewById<TextView>(Resource.Id.usernameText))
            {
                usernameText.Text = username;
            }
        }

        /// <summary>
        /// Replaces the fragment.
        /// </summary>
        /// <param name="fragment">Fragment.</param>
        /// <param name="name">Name.</param>
        public void ReplaceFragment(Fragment fragment, string name)
        {

            SupportFragmentManager.BeginTransaction()
                                       .Replace(Resource.Id.fragment_container, fragment, name)
                                       .CommitAllowingStateLoss();
        }

        /// <summary>
        /// Requests the sync.
        /// </summary>
        public void RequestSync()
        {
            ContentResolver.RequestSync(_account, AccountUtils.SyncAuthority, Bundle.Empty);
        }
    }
}

