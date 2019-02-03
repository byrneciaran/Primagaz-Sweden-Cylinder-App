using System;
using System.Threading.Tasks;
using Primagaz.Standard.Entities;
using System.Linq;
using Primagaz.Standard.Service;
using CommonServiceLocator;

namespace Primagaz.Standard
{
    public static class Authenticator
    {
        /// <summary>
        /// Authenticate the subscriber
        /// </summary>
        /// <returns>The authenticate.</returns>
        /// <param name="username">Subscriber identifier.</param>
        /// <param name="password">Password.</param>
        async public static Task<AuthenticationResult> Authenticate(Repository repository,string username, string password)
        {
            var syncResult = new SyncResult();

            var logger = ServiceLocator.Current.GetInstance<IDeviceManager>();

            var currentProfile = repository.Profiles.FirstOrDefault();

            // no profile or different profile - perform an initial sync
            if (currentProfile == null || !currentProfile.SubscriberID.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                // perform an initial sync
                logger.LogEvent("Perform initial sync");
                syncResult = await ApiClient.Instance.PerfomInitialSyncAsync(username, password);

                if (syncResult.IsSuccessStatusCode)
                    return AuthenticationResult.Authenticated;

                return syncResult.StatusCode == System.Net.HttpStatusCode.RequestTimeout ? AuthenticationResult.Timeout :
                                 AuthenticationResult.RemoteAuthenticationFailed;
            }


            return !currentProfile.Password.Equals(password, StringComparison.OrdinalIgnoreCase) ? AuthenticationResult.InvalidPassword :
                 AuthenticationResult.Authenticated;


        }
    }

    public enum AuthenticationResult { InvalidPassword, Authenticated, RemoteAuthenticationFailed, Timeout };
}
