using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Util;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Primagaz.Standard;
using static Android.Provider.Settings;

namespace Primagaz.Android
{
    public class DeviceManager : IDeviceManager
    {
        public void LogException(Exception exception, IDictionary<string, string> properties = null)
        {
            Crashes.TrackError(exception, properties);
        }

        public void LogEvent(string name, IDictionary<string, string> properties = null)
        {
            Analytics.TrackEvent(name, properties);
        }

        public void LogMessage(string tag, string message)
        {
            Log.Info(tag, message);
        }

        public string GetUniqueDeviceId()
        {
            var context = Application.Context;
            return Secure.GetString(context.ContentResolver, Secure.AndroidId);
        }
    }
}
