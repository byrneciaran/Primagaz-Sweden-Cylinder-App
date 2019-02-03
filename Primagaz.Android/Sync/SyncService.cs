using Android.App;
using Android.Content;
using Android.OS;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using Primagaz.Standard;

namespace Primagaz.Android.Sync
{
    [Service(Exported = true, Process = ":primagaz_cylinder_sync", Enabled = true, 
    Permission = "android.permission.BIND_JOB_SERVICE",Label = "Primagaz Sync Service")]
    //[Service(Exported = true)]
    [IntentFilter(new[] { "android.content.SyncAdapter" })]
    [MetaData("android.content.SyncAdapter", Resource = "@xml/syncadapter")]
    public class SyncService : Service
    {
        static SyncAdapter _syncAdapter;
        static readonly object _syncAdapterLock = new object();

        /// <summary>
        /// Create the sync adapter as a singleton.  Set the sync adapter as syncable
        /// Disallow parallel syncs
        /// </summary>
        public override void OnCreate()
        {
            RegisterServiceLocator();

            lock (_syncAdapterLock)
            {
                _syncAdapter = new SyncAdapter(ApplicationContext, true);
            }
        }

        /// <summary>
        /// Registers the service locator.
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
        /// Get the object that allows external processes to call onPerformSync(). 
        /// The object is created in the base class code when the SyncAdapter
        /// constructors call super()
        /// </summary>
        /// <returns>The bind.</returns>
        /// <param name="intent">Intent.</param>
        public override IBinder OnBind(Intent intent)
        {
            return _syncAdapter.SyncAdapterBinder;
        }
    }
}
