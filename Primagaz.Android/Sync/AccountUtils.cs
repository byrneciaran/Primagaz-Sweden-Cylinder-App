using Android.Accounts;
using Android.Content;

namespace Primagaz.Android.Sync
{
	public static class AccountUtils
    {
        public const string SyncAuthority = "se.primagaz.cylinder.provider";
        public const string AccountType = "se.primagaz.cylinder";
        public const string Account = "primagaz";

        public static Account CreateSyncAccount(Context context)
        {
            Account newAccount = new Account(Account, AccountType);

            AccountManager accountManager = (AccountManager)context.GetSystemService(Context.AccountService);
            accountManager.AddAccountExplicitly(newAccount, null, null);

            return newAccount;
        }
    }
}
