using System;
using System.Linq;
using Primagaz.Standard.Entities;

namespace Primagaz.Standard.Service
{
    public static class ProfileService
    {
        /// <summary>
        /// Sets the current run.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="runNumber">Run number.</param>
        public static void SetCurrentRun(Repository repository, string runNumber)
        {
            var profile = repository.Profiles.FirstOrDefault();
            profile.CurrentRunNumber = runNumber;
            repository.SaveChanges();
        }

        /// <summary>
        /// Sets the current printer.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="printerAddress">Printer address.</param>
        public static void SetCurrentPrinter(Repository repository, string printerAddress)
        {
            var mobileDevice = repository.MobileDevices.First();
            mobileDevice.PrinterAddress = printerAddress;
            repository.SaveChanges();
        }

        /// <summary>
        /// Sets the current trailer.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="trailer">Trailer.</param>
        public static void SetCurrentTrailer(Repository repository, Trailer trailer)
        {
            var profile = repository.Profiles.FirstOrDefault();
            profile.CurrentTrailerNumber = trailer.TrailerNumber;
            repository.SaveChanges();
        }
    }
}
