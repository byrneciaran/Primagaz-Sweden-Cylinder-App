using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using Primagaz.Standard;
using Primagaz.Standard.Service;
using Xamarin.Essentials;

namespace Primagaz.Android
{
    public class LoginFragment : BaseFragment
    {
        public static readonly string TAG = typeof(LoginFragment).FullName;

        Button _loginButton;
        EditText _usernameText;
        EditText _passwordText;
        TextView _versionTextView;

        Repository _repository;

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <returns>The instance.</returns>
        public static LoginFragment NewInstance()
        {
            var fragment = new LoginFragment { Arguments = new Bundle() };
            return fragment;
        }

        /// <summary>
        /// Destroy View
        /// </summary>
        public override void OnDestroyView()
        {
            _loginButton.Click -= OnLogin;
            _versionTextView.Click -= OnReset;

            _repository.Dispose();

            base.OnDestroyView();
        }

        /// <summary>
        /// Create View Event
        /// </summary>
        /// <returns>The create view.</returns>
        /// <param name="inflater">Inflater.</param>
        /// <param name="container">Container.</param>
        /// <param name="savedInstanceState">Saved instance state.</param>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.login_fragment, container, false);

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
            Activity.ActionBar.Hide();

            _loginButton = view.FindViewById<Button>(Resource.Id.loginButton);
            _loginButton.Click += OnLogin;

            _usernameText = view.FindViewById<EditText>(Resource.Id.usernameText);
            _passwordText = view.FindViewById<EditText>(Resource.Id.passwordText);

            _versionTextView = view.FindViewById<TextView>(Resource.Id.versionTextView);
            _versionTextView.Text = String.Format("Version: {0}.{1}", VersionTracking.CurrentVersion,
                                                  VersionTracking.CurrentBuild);

            _versionTextView.Click += OnReset;


#if DEBUG
            _usernameText.Text = "mit1";
            _passwordText.Text = "mit1";
#endif
        }


        /// <summary>
        /// Login Click Event
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnLogin(object sender, EventArgs e)
        {
            var username = _usernameText.Text;
            var password = _passwordText.Text;

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
            {
                var message = Resources.GetString(Resource.String.message_no_credentials);
                UserDialogs.Instance.Alert(message);
                return;
            }

            var title = Resources.GetString(Resource.String.message_loading);
            UserDialogs.Instance.ShowLoading(title);

            var result = await Authenticator.Authenticate(_repository, username, password);

            UserDialogs.Instance.HideLoading();

            switch (result)
            {
                case AuthenticationResult.Authenticated:
                    OnLoginSucceeded(username);
                    break;
                default:
                    OnLoginFailed(result);
                    break;
            }

        }

        async void OnReset(object sender, EventArgs e)
        {
            var promptConfig = new PromptConfig
            {
                Message = "Enter support code",
                InputType = InputType.Number
            };

            var promptResult = await UserDialogs.Instance.PromptAsync(promptConfig);

            if (promptResult.Ok)
            {
                var otp = promptResult.Text;

                if (!OtpService.ValidateOTP(otp))
                    return;

                Activity.RunOnUiThread(() =>
                {
                    UserDialogs.Instance.ShowLoading();
                });

                try
                {
                    // upload the realm
                    await SupportUtils.UploadDatabase();

                    Activity.RunOnUiThread(() =>
                    {
                        UserDialogs.Instance.Alert("Reset Complete. Please Login.");
                    });
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.Alert("Reset Failed.");
                    Crashes.TrackError(ex);

                }
                finally
                {
                    Activity.RunOnUiThread(UserDialogs.Instance.HideLoading);
                }
            }
        }


        /// <summary>
        /// Login failed
        /// </summary>
        void OnLoginFailed(AuthenticationResult result)
        {
            if (Activity == null || !IsAdded)
                return;

            _usernameText.Text = null;
            _passwordText.Text = null;

            int error;

            switch (result)
            {
                case AuthenticationResult.Timeout:
                    error = Resource.String.message_timeout;
                    break;
                case AuthenticationResult.InvalidPassword:
                    error = Resource.String.message_invalid_password;
                    break;
                default:
                    error = Resource.String.message_invalid_username;
                    break;
            }

            var message = Resources.GetString(error);
            UserDialogs.Instance.Alert(message);
        }

        /// <summary>
        /// Login succeeded
        /// </summary>
        void OnLoginSucceeded(string username)
        {

            _fragmentActionListener.SetDisplayUsername(username);

            var fragment = RunsFragment.NewInstance();
            _fragmentActionListener.ReplaceFragment(fragment, RunsFragment.TAG);
        }

    }
}
