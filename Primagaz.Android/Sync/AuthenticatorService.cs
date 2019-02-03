using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace Primagaz.Android.Sync
{
	[Service]
    [IntentFilter(new[] { "android.accounts.AccountAuthenticator" })]
    [MetaData("android.accounts.AccountAuthenticator", Resource = "@xml/authenticator")]
    public class AuthenticatorService:Service
    {
        Authenticator _authenticator;

        public override void OnCreate()
        {
            _authenticator = new Authenticator(this);
        }

        public override IBinder OnBind(Intent intent)
        {
            return _authenticator.IBinder;
        }
    }
}
