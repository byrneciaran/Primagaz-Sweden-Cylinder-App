using Android.Accounts;
using Android.Content;

namespace Primagaz.Android.Sync
{
	public static class AccountUtils
    {
        public const string SyncAuthority = "io.clickahead.primagaz.provider";
        public const string AccountType = "io.clickahead.primagaz";
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
