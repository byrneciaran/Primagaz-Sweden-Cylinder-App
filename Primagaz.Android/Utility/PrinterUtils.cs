using System;
using System.Threading;
using Android.App;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Graphics;
using Zebra.Sdk.Printer;


namespace Primagaz.Android
{
    public enum PrintStatus { Success, NotFound, NotReady, PrintFailed, PrintException };

    public class PrintResult
    {
        public PrintStatus Status { get; set; }
        public string Message { get; set; }
    }

    public static class PrinterUtils
    {
        public const string TAG = "Primagaz.PrinterUtils";

        // 10,000 Milliseconds = 10 Seconds
        const int SignaturePauseMilliseconds = 1000;

        const string SignatureFilename = "SIG.pcx";

        const int InitialiseResponseTimeoutMilliseconds = 3000;
        const int ResponseCompletionTimeoutMilliseconds = 1000;

        public static PrintResult Print(string content, string address)
        {
            return Print(content, address, null);
        }

        /// <summary>
        /// Set Printer Parameters
        /// </summary>
        /// <returns>The printer.</returns>
        /// <param name="address">Address.</param>
        public static PrintResult SetDefaultPrinterSettings(string address)
        {
            var result = new PrintResult();

            IConnection connection = null;

            try
            {

                connection = new BluetoothConnectionInsecure(address);
                connection.Open();

                using (var printer = ZebraPrinterFactory.GetInstance(PrinterLanguage.Zpl, connection))
                {

                    // set printer to bluetooth classic only
                    printer.SendCommand("! U1 setvar \"bluetooth.le.controller_mode\" \"classic\"" + "\r\n");

                    // disable wifi
                    printer.SendCommand("! U1 setvar \"wlan.enable\" \"off\"" + "\r\n");

                    // disable wifi power save
                    printer.SendCommand("! U1 setvar \"wlan.power_save\" \"off\"" + "\r\n");

                    // disable bonding
                    printer.SendCommand("! U1 setvar \"bluetooth.bonding\" \"off\"" + "\r\n");

                    // disable sleep mode
                    printer.SendCommand("! U1 setvar \"power.sleep.enable\" \"off\"" + "\r\n");

                    // never power down due to inactivity
                    printer.SendCommand("! U1 setvar \"power.inactivity_timeout\" \"0\"" + "\r\n");

                    // set sleep timeout to 8 hours - not sure if sleep.enable = off overrides this?
                    printer.SendCommand("! U1 setvar \"power.sleep.timeout\" \"28000\"" + "\r\n");

                }

                result.Status = PrintStatus.Success;

                return result;
            }
            catch (Exception exception)
            {
                Microsoft.AppCenter.Crashes.Crashes.TrackError(exception);
                result.Message = "Print failed. Check the printer is switched on and try again.";
                result.Status = PrintStatus.PrintException;

                return result;
            }
            finally
            {
                Thread.Sleep(SignaturePauseMilliseconds);
                connection.Close();
            }

        }

        /// <summary>
        /// Print content
        /// </summary>
        /// <returns>The print.</returns>
        /// <param name="content">Content.</param>
        public static PrintResult Print(string content, string address, string signature)
        {
            var result = new PrintResult
            {
                Status = PrintStatus.PrintFailed
            };

            IConnection connection = null;

            try
            {
                connection = new BluetoothConnectionInsecure(address);
                connection.Open();

                var printer = ZebraPrinterFactory.GetInstance(PrinterLanguage.Cpcl, connection);

                // if there is a signature store it
                if (signature != null)
                {
                    using (var image = ZebraImageFactory.GetImage(signature))
                    {
                        printer.StoreImage(SignatureFilename, image, image.Width / 4, image.Height / 4);
                    }

                    Thread.Sleep(SignaturePauseMilliseconds);
                }


                // write the label
                var printLabel = ConvertExtendedAscii(content);

                connection.SendAndWaitForResponse(printLabel, InitialiseResponseTimeoutMilliseconds,
                                                  ResponseCompletionTimeoutMilliseconds, null);

                var status = printer.CurrentStatus;

                while (status.NumberOfFormatsInReceiveBuffer > 0 && status.IsReadyToPrint)
                {
                    Thread.Sleep(500);
                    status = printer.CurrentStatus;
                }

                if (!status.IsReadyToPrint)
                    LogPrinterFailure(result, status);

                result.Status = PrintStatus.Success;

                printer.SendCommand("! U1 setvar \"formats.cancel_all\" \"\"" + "\r\n");

                return result;
            }

            catch (Exception exception)
            {
                var context = Application.Context;
                var message = context.Resources.GetString(Resource.String.message_print_failed_continue);

                Microsoft.AppCenter.Crashes.Crashes.TrackError(exception);
                result.Message = message;
                result.Status = PrintStatus.PrintException;
                return result;
            }
            finally
            {
                connection.Close();
            }

        }

        /// <summary>
        /// Log printer failure
        /// </summary>
        /// <param name="result">Result.</param>
        /// <param name="status">Status.</param>
        static void LogPrinterFailure(PrintResult result, PrinterStatus status)
        {
            if (status.IsHeadOpen)
                result.Message = "Cannont print because the printer head is open.";
            else if (status.IsHeadCold)
                result.Message = "Cannot print because the printer head is cold.";
            else if (status.IsHeadTooHot)
                result.Message = "Cannot print because the printer head is too hot.";
            else if (status.IsPaperOut)
                result.Message = "Cannot print because the paper is out.";
            else if (status.IsPartialFormatInProgress)
                result.Message = "Cannot print because a partial format is in progress.";
            else if (status.IsPaused)
                result.Message = "Cannot print because the printer is paused.";
            else if (status.IsReceiveBufferFull)
                result.Message = "Cannot print because the receive buffer is full.";
            else if (status.IsRibbonOut)
                result.Message = "Cannot print because the ribbon is out.";

            var printFailureException = new PrintFailureException(result.Message);
            Microsoft.AppCenter.Crashes.Crashes.TrackError(printFailureException);
        }

        public static byte[] ConvertExtendedAscii(String input)
        {
            int length = input.Length;

            char[] characters = input.ToCharArray();
            byte[] retVal = new byte[length];

            for (int i = 0; i < length; i++)
            {
                char c = characters[i];

                retVal[i] = c < 127 ? (byte)c : (byte)(c - 256);
            }

            return retVal;
        }
    }

    public class PrintFailureException:Exception
    {
        public PrintFailureException(string message):base(message)
        {

        }
    }
}
