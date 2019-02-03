
using System;
using System.Collections.Generic;

namespace Primagaz.Standard
{
    public interface IDeviceManager
    {
        void LogEvent(string name, IDictionary<string, string> properties = null);
        void LogException(Exception exception, IDictionary<string, string> properties = null);
        void LogMessage(string tag, string message);
        string GetUniqueDeviceId();
    }
}
