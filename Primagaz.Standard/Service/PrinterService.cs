using System.Linq;
using Primagaz.Standard.Entities;

namespace Primagaz.Standard.Service
{
    public static class PrinterService
    {

        /// <summary>
        /// Set printer
        /// </summary>
        /// <param name="realm">Realm.</param>
        /// <param name="friendlyName">Friendly name.</param>
        /// <param name="address">Address.</param>
        public static void SetDefaultPrinter(Repository repository, string friendlyName,string address)
        {
            var printer = repository.Printers.FirstOrDefault();

            if (printer != null)
                repository.Printers.Remove(printer);

            printer = new Printer
            {
                Address = address,
                FriendlyName = friendlyName
            };

            repository.Printers.Add(printer);
            repository.SaveChanges();
        }


    }
}
