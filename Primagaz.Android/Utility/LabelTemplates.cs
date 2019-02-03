using System;
using System.Text;
using System.Collections.Generic;
using Primagaz.Standard.Entities;
using System.Globalization;
using System.Linq;
using System.Collections;
using System.Drawing;

namespace Primagaz.Android
{
    public static class LabelTemplates
    {
        const int DpiRatio = 80;
        static readonly CultureInfo LabelCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets the denmark lending label.
        /// </summary>
        /// <returns>The denmark lending label.</returns>
        /// <param name="lending">Lending.</param>
        public static string GetDenmarkLendingLabel(List<LendingStatus> lending)
        {
            const decimal dcItemVerticalOffset = 0.4M;
            const decimal dcSmallLineOffset = 0.1M;
            const decimal dcLargeLineOffset = 0.3M;

            // Print Commands
            // <i>{offset}<200><200>{height}{qty}
            // <200>: Horizontal & Vertical resolution (in dots-per-inch)
            var label = new StringBuilder("! 0 200 200 #Length 1\r\n");
            label.Append("JOURNAL\r\n"); // Disable check for correct media alignment
            label.Append("SPEED 5\r\n"); // Set print speed between 0 and 5, 0 being the slowest
            label.Append("BAR-SENSE\r\n"); // Intruct printer as to means of top-of-form detection
            label.Append("ENCODING UTF-8\r\n");
            label.Append("COUNTRY DENMARK\r\n");
            label.Append("IN-CENTIMETERS\r\n");
            label.Append("CENTER\r\n");

            // Initialise the Vertical Offset at 7.6cm
            var dcVerticalOffset = 0M;

            dcVerticalOffset = dcVerticalOffset + 1.0M;
            label.Append("LEFT\r\n");
            label.AppendFormat(LabelCulture, "L 0.5 {0} 10 {0} 0.03\r\n", dcVerticalOffset);

            dcVerticalOffset = dcVerticalOffset + dcItemVerticalOffset;
            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", dcVerticalOffset, "Før denne levering er udlånssaldoen:");

            dcVerticalOffset = dcVerticalOffset + 0.2M;
            dcVerticalOffset = dcVerticalOffset + (dcItemVerticalOffset * 2);

            //dcVerticalOffset = dcVerticalOffset + (dcItemVerticalOffset * 2);
            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", dcVerticalOffset, "Flaske");
            label.Append("RIGHT 4\r\n");
            label.AppendFormat(LabelCulture, "T 5 0 2.3 {0} {1}\r\n", dcVerticalOffset, "Stk");

            dcVerticalOffset = dcVerticalOffset + dcLargeLineOffset;
            label.Append("LEFT\r\n");
            label.AppendFormat(LabelCulture, "L 0.5 {0} 5 {0} 0.03\r\n", dcVerticalOffset);
            dcVerticalOffset = dcVerticalOffset + dcSmallLineOffset;

            foreach (var status in lending.Where(x => x.Quantity != 0))
            {
                label.Append("LEFT\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", dcVerticalOffset,
                    status.ProductCode);
                label.Append("RIGHT 4\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 2.3 {0} {1}\r\n", dcVerticalOffset, status.Quantity);

                // Increment the Vertical Position by the Vertical Offset
                dcVerticalOffset = dcVerticalOffset + dcItemVerticalOffset;
            }

            label.Append("LEFT\r\n");
            dcVerticalOffset = dcVerticalOffset + dcSmallLineOffset;
            label.AppendFormat(LabelCulture, "L 0.5 {0} 5 {0} 0.03\r\n", dcVerticalOffset);

            dcVerticalOffset = dcVerticalOffset + dcLargeLineOffset;
            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", dcVerticalOffset, "Saldo");

            label.Append("RIGHT 4\r\n");
            label.AppendFormat(LabelCulture, "T 5 0 2.3 {0} {1}\r\n", dcVerticalOffset,
                lending.Sum(x => x.Quantity));

            label.Append("LEFT\r\n");
            dcVerticalOffset = dcVerticalOffset + dcLargeLineOffset;
            label.AppendFormat(LabelCulture, "L 0.5 {0} 5 {0} 0.03\r\n", dcVerticalOffset);

            dcVerticalOffset = dcVerticalOffset + dcItemVerticalOffset;
            label.AppendFormat(LabelCulture, "T 5 0.5 0.5 {0} {1}\r\n", dcVerticalOffset,
                               "Kontakt os venligst indenfor 1 uge på kundeservice@primagaz.dk,");

            dcVerticalOffset = dcVerticalOffset + dcItemVerticalOffset;
            label.AppendFormat(LabelCulture, "T 5 0.5 0.5 {0} {1}\r\n", dcVerticalOffset,
                               "hvis udlånsbeholdningen ikke stemmer");

            dcVerticalOffset = dcVerticalOffset + dcItemVerticalOffset;
            label.AppendFormat(LabelCulture, "T 5 0.5 0.5 {0} {1}\r\n", dcVerticalOffset,
                               "Ellers betragtes udlånssaldoen som gældende.");

            dcVerticalOffset = dcVerticalOffset + dcItemVerticalOffset;

            dcVerticalOffset = dcVerticalOffset + (dcItemVerticalOffset * 4);

            label.Replace("#Length", (dcVerticalOffset * 80).ToString(CultureInfo.InvariantCulture));
            label.Append("PRINT\r\n");

            return label.ToString();
        }


        /// <summary>
        /// Gets the docket label.
        /// </summary>
        /// <returns>The docket label.</returns>
        /// <param name="docket">Docket.</param>
        /// <param name="docketItems">Docket items.</param>
        /// <param name="lendingStatuses">Lending statuses.</param>
        public static string GetDocketLabel(DeliveryDocket docket, List<DeliveryDocketItem> docketItems,
                                                   List<LendingStatus> lendingStatuses)
        {
            const decimal dcItemVerticalOffset = 0.4M;
            const decimal dcSmallLineOffset = 0.1M;
            const decimal dcLargeLineOffset = 0.3M;
            const decimal dcFooter = 2.4M;

            // Print Commands
            // <i>{offset}<200><200>{height}{qty}
            // <200>: Horizontal & Vertical resolution (in dots-per-inch)
            var label = new StringBuilder("! 0 200 200 #Length 1\r\n");
            label.Append("JOURNAL\r\n"); // Disable check for correct media alignment
            label.Append("SPEED 5\r\n"); // Set print speed between 0 and 5, 0 being the slowest
            label.Append("BAR-SENSE\r\n"); // Intruct printer as to means of top-of-form detection
            label.Append("ENCODING UTF-8\r\n");
            label.Append("COUNTRY DENMARK\r\n");
            label.Append("IN-CENTIMETERS\r\n");
            label.Append("CENTER\r\n");

            label.AppendFormat(LabelCulture, "T 4 0 0 0.1 {0}\r\n", "Primagaz Danmark A/S");
            label.AppendFormat(LabelCulture, "T 4 0 0 0.8 {0}\r\n", "Følgeseddel");

            label.Append("LEFT\r\n");
            label.AppendFormat(LabelCulture, "L 0.5 1.7 10 1.7 0.05\r\n");

            // Header - Line 1
            label.AppendFormat(LabelCulture, "T 5 0 0.5 2.1 {0}:\r\n", "Dok. nr.");
            label.AppendFormat(LabelCulture, "T 5 0 2.7 2.1 {0}\r\n", docket.DocketID);
            label.AppendFormat(LabelCulture, "T 5 0 5.8 2.1 {0}:\r\n", "Afsender");

            // Header - Line 2
            label.AppendFormat(LabelCulture, "T 5 0 0.5 2.5 {0}:\r\n", "Dato");
            label.AppendFormat(LabelCulture, "T 5 0 2.7 2.5 {0}\r\n", docket.FormattedDateModified);
            label.AppendFormat(LabelCulture, "T 5 0 5.8 2.5 {0}\r\n", docket.SubscriberDisplayName);

            // Header - Line 3
            label.AppendFormat(LabelCulture, "T 5 0 0.5 2.9 {0}:\r\n", "Tur");
            label.AppendFormat(LabelCulture, "T 5 0 2.7 2.9 {0}\r\n", docket.RunNumber);
            label.AppendFormat(LabelCulture, "T 5 0 5.8 2.9 {0}\r\n", docket.SubscriberAddress);

            // Header - Line 4
            label.AppendFormat(LabelCulture, "T 5 0 0.5 3.3 {0}:\r\n", "Order");
            label.AppendFormat(LabelCulture, "T 5 0 2.7 3.3 {0}\r\n", docket.OrderNumber);
            label.AppendFormat(LabelCulture, "T 5 0 5.8 3.3 {0} {1}\r\n", docket.SubscriberPostCode,
                docket.SubscriberCity);

            // Header - Line 5
            label.AppendFormat(LabelCulture, "T 5 0 0.5 3.7 {0}:\r\n", "Kunde rekv");
            label.AppendFormat(LabelCulture, "T 5 0 2.7 3.7 {0}\r\n", docket.OrderReference);

            label.AppendFormat(LabelCulture, "L 0.5 4.1 10 4.1 0.05\r\n");

            label.AppendFormat(LabelCulture, "T 5 0 0.5 4.5 {0}:\r\n", "Modtager");
            label.AppendFormat(LabelCulture, "T 5 0 2.7 4.5 {0}\r\n", docket.CustomerAccountNumber);
            label.AppendFormat(LabelCulture, "T 5 0 2.7 4.9 {0}\r\n", docket.CustomerName1);
            label.AppendFormat(LabelCulture, "T 5 0 2.7 5.3 {0}\r\n", docket.AddressLine1);
            label.AppendFormat(LabelCulture, "T 5 0 2.7 5.7 {0} {1}\r\n", docket.AddressLine2,
                docket.AddressLine3);

            label.AppendFormat(LabelCulture, "L 0.5 6.3 10 6.3 0.05\r\n");


            // Initialise the Vertical Offset at 7.6cm
            var verticalOffset = 6.2M;

   
            decimal? emptiesDividerStart = null;

            var hasFullsFaulty = docketItems.Any(x => x.FaultyFulls > 0);
            var hasFullsCollected = docketItems.Any(x => x.FullsCollected > 0);

            if (docketItems.Any())
            {
                verticalOffset = verticalOffset + dcItemVerticalOffset;
                label.AppendFormat(LabelCulture, "T 4 0 0.5 {0} {1}\r\n", verticalOffset, "Leverance");
                label.Append("CENTER 2.9\r\n");

                verticalOffset = verticalOffset + dcItemVerticalOffset;

                emptiesDividerStart = verticalOffset;

                verticalOffset = verticalOffset + dcItemVerticalOffset;
                label.Append("CENTER 4\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 2.8 {0} {1}\r\n", verticalOffset, "Fulde");

                if (hasFullsFaulty)
                {
                    label.Append("CENTER 5.55\r\n");
                    label.AppendFormat(LabelCulture, "T 5 0 4.8 {0} {1}\r\n", verticalOffset, "Fulde");
                }

                if (hasFullsCollected)
                {
                    label.Append("CENTER 6.95\r\n");
                    label.AppendFormat(LabelCulture, "T 5 0 6.45 {0} {1}\r\n", verticalOffset, "Fulde");
                }

                label.Append("CENTER 8.55\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 8.3 {0} {1}\r\n", verticalOffset, "Tom");

                label.Append("LEFT\r\n");
                verticalOffset = verticalOffset + dcItemVerticalOffset;
                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", verticalOffset, "Produkt");


                label.Append("CENTER 4\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 2.8 {0} {1}*\r\n", verticalOffset, "lev.");

                if (hasFullsFaulty)
                {
                    label.Append("CENTER 5.5\r\n");
                    label.AppendFormat(LabelCulture, "T 5 0 4.8 {0} {1}*\r\n", verticalOffset, "defekte");
                }

                if (hasFullsCollected)
                {
                    label.Append("CENTER 6.95\r\n");
                    label.AppendFormat(LabelCulture, "T 5 0 6.45 {0} {1}*\r\n", verticalOffset, "afh.");
                }

                label.Append("CENTER 9.5\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 7.8 {0} {1}\r\n", verticalOffset, "afh.");

                verticalOffset = verticalOffset + dcLargeLineOffset;
                label.Append("LEFT\r\n");
                label.AppendFormat("L 0.5 {0} 10 {0} 0.01\r\n", verticalOffset);


                verticalOffset = verticalOffset + dcSmallLineOffset;

                // 13/06/13 - Fixed: Fulls Collected not showing
                foreach (
                    var fill in
                        docketItems.Where(
                            x =>
                                x.FullsDelivered > 0 || x.EmptiesCollected > 0 || x.FullsCollected > 0 ||
                                x.FaultyFulls > 0))
                {
                    label.Append("LEFT\r\n");

                    //if (fill.DeliveryTypeID == 0)
                    //    label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", dcVerticalOffset, fill.Description);
                    //else
                    label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1} - {2}\r\n", verticalOffset, fill.ProductCode, fill.Description);

                    label.Append("CENTER 4\r\n");
                    label.AppendFormat(LabelCulture, "T 5 0 2.8 {0} {1}\r\n", verticalOffset, fill.FullsDelivered);

                    if (hasFullsFaulty)
                    {
                        label.Append("CENTER 5.5\r\n");
                        label.AppendFormat(LabelCulture, "T 5 0 4.8 {0} {1}\r\n", verticalOffset, fill.FaultyFulls);
                    }

                    if (hasFullsCollected)
                    {
                        label.Append("CENTER 6.95\r\n");
                        label.AppendFormat(LabelCulture, "T 5 0 6.45 {0} {1:G0}\r\n", verticalOffset,
                            fill.FullsCollected);
                    }

                    label.Append("CENTER 9.5\r\n");
                    label.AppendFormat(LabelCulture, "T 5 0 7.8 {0} {1:G0}\r\n", verticalOffset,
                        fill.EmptiesCollected);

                    // Increment the Vertical Position by the Vertical Offset
                    verticalOffset = verticalOffset + dcItemVerticalOffset;
                }

                label.AppendFormat(LabelCulture, "L 0.5 {0} 10 {0} 0.03\r\n", verticalOffset);
                verticalOffset = verticalOffset + dcItemVerticalOffset;

                // Totals
                label.Append("LEFT\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", verticalOffset, "Saldo");
                label.Append("CENTER 4\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 2.8 {0} {1}\r\n", verticalOffset,
                    docketItems.Sum(x => x.FullsDelivered));

                if (hasFullsFaulty)
                {
                    label.Append("CENTER 5.5\r\n");
                    label.AppendFormat(LabelCulture, "T 5 0 4.8 {0} {1}*\r\n", verticalOffset,
                        docketItems.Sum(x => x.FaultyFulls));
                }

                if (hasFullsCollected)
                {
                    label.Append("CENTER 6.95\r\n");
                    label.AppendFormat(LabelCulture, "T 5 0 6.35 {0} {1:G0}\r\n", verticalOffset,
                        docketItems.Sum(x => x.FullsCollected));
                }

                label.Append("CENTER 9.5\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 7.8 {0} {1:G0}\r\n", verticalOffset,
                    docketItems.Sum(x => x.EmptiesCollected));
            }

            label.Append("LEFT\r\n");

            verticalOffset = verticalOffset + dcItemVerticalOffset;
            label.AppendFormat(LabelCulture, "L 0.5 {0} 10 {0} 0.03\r\n", verticalOffset);

            label.AppendFormat(LabelCulture, "L 7.8 {0} 7.8 {1} 0.03\r\n", emptiesDividerStart, verticalOffset);

            verticalOffset = verticalOffset + (dcItemVerticalOffset);

            label.Append("LEFT\r\n");
            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", verticalOffset, "* UN 1965, Carbonhydrid gasblanding, fordråbet, N.O.S., 2.1, (B/D)");
            verticalOffset = verticalOffset + dcItemVerticalOffset;

            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", verticalOffset, "* Gasflasker");

            verticalOffset = verticalOffset + (dcItemVerticalOffset * 2);
            label.Append("LEFT\r\n");

            if (docket.SignaturePath != null)
            {
                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", verticalOffset, "Kundens underskrift");
                label.AppendFormat(LabelCulture, "T 5 0 4.8 {0} {1}\r\n", verticalOffset, "Chaufførs ID");

                verticalOffset = verticalOffset + dcItemVerticalOffset;
                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", verticalOffset, docket.CustomerPrintName);
                label.AppendFormat(LabelCulture, "T 5 0 4.8 {0} {1}\r\n", verticalOffset, docket.DriverPrintName);

                //int iY;

                // PCX 100 134 !<nameFile.pcx\r\n
                verticalOffset = verticalOffset + dcItemVerticalOffset;
                label.AppendFormat(LabelCulture, "PCX 2 {0} !<SIG.pcx\r\n", verticalOffset);
                verticalOffset = verticalOffset + 2.0M;

            }

            if (lendingStatuses.Any() && docket.ShowLendingStatus)
            {
                verticalOffset = verticalOffset + 1.0M;
                label.Append("LEFT\r\n");
                label.AppendFormat(LabelCulture, "L 0.5 {0} 10 {0} 0.03\r\n", verticalOffset);

                verticalOffset = verticalOffset + dcItemVerticalOffset;
                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", verticalOffset, "Før denne levering er udlånssaldoen:");

                verticalOffset = verticalOffset + 0.2M;
                verticalOffset = verticalOffset + (dcItemVerticalOffset * 2);

                //dcVerticalOffset = dcVerticalOffset + (dcItemVerticalOffset * 2);
                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", verticalOffset, "Flaske");
                label.Append("RIGHT 4\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 2.3 {0} {1}\r\n", verticalOffset, "Stk");

                verticalOffset = verticalOffset + dcLargeLineOffset;
                label.Append("LEFT\r\n");
                label.AppendFormat(LabelCulture, "L 0.5 {0} 5 {0} 0.03\r\n", verticalOffset);
                verticalOffset = verticalOffset + dcSmallLineOffset;

                foreach (var status in lendingStatuses.Where(x => x.Quantity != 0))
                {
                    label.Append("LEFT\r\n");
                    label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", verticalOffset,
                        status.ProductCode);
                    label.Append("RIGHT 4\r\n");
                    label.AppendFormat(LabelCulture, "T 5 0 2.3 {0} {1}\r\n", verticalOffset, status.Quantity);

                    // Increment the Vertical Position by the Vertical Offset
                    verticalOffset = verticalOffset + dcItemVerticalOffset;
                }

                label.Append("LEFT\r\n");
                verticalOffset = verticalOffset + dcSmallLineOffset;
                label.AppendFormat(LabelCulture, "L 0.5 {0} 5 {0} 0.03\r\n", verticalOffset);

                verticalOffset = verticalOffset + dcLargeLineOffset;
                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", verticalOffset, "Saldo");

                label.Append("RIGHT 4\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 2.3 {0} {1}\r\n", verticalOffset,
                    lendingStatuses.Sum(x => x.Quantity));

                label.Append("LEFT\r\n");
                verticalOffset = verticalOffset + dcLargeLineOffset;
                label.AppendFormat(LabelCulture, "L 0.5 {0} 5 {0} 0.03\r\n", verticalOffset);

                verticalOffset = verticalOffset + dcItemVerticalOffset;
                label.AppendFormat(LabelCulture, "T 5 0.5 0.5 {0} {1}\r\n", verticalOffset,
                                   "Kontakt os venligst indenfor 1 uge på kundeservice@primagaz.dk,");

                verticalOffset = verticalOffset + dcItemVerticalOffset;
                label.AppendFormat(LabelCulture, "T 5 0.5 0.5 {0} {1}\r\n", verticalOffset,
                                   "hvis udlånsbeholdningen ikke stemmer");

                verticalOffset = verticalOffset + dcItemVerticalOffset;
                label.AppendFormat(LabelCulture, "T 5 0.5 0.5 {0} {1}\r\n", verticalOffset,
                                   "Ellers betragtes udlånssaldoen som gældende.");

                verticalOffset = verticalOffset + dcItemVerticalOffset;

                if (docket.Confirmed)
                    label.AppendFormat(LabelCulture, "T 5 0 4.3 {0} *** {1} ***\r\n", verticalOffset,
                                       "Confirmed");

                verticalOffset = verticalOffset + (dcItemVerticalOffset * 4);
            }
            else
            {
                // Print Confirmed Message
                if (docket.Confirmed)
                {
                    // Add the Footer
                    verticalOffset = verticalOffset + dcFooter;
                    label.AppendFormat(LabelCulture, "T 5 0 4.3 {0} *** {1} ***\r\n", verticalOffset,
                                       "Confirmed");

                    // Triple Space
                    verticalOffset = verticalOffset + (dcItemVerticalOffset * 4);
                }
                else
                    // Add the Footer
                    verticalOffset = verticalOffset + dcFooter;
            }

            label.Replace("#Length", (verticalOffset * 80).ToString(CultureInfo.InvariantCulture));
            label.Append("PRINT\r\n");

            return label.ToString();
        }

        /// <summary>
        /// Gets the on stop label.
        /// </summary>
        /// <returns>The on stop label.</returns>
        /// <param name="customers">Customers.</param>
        public static string GetDenmarkOnStopLabel(List<Customer> customers)
        {
            const decimal margin = 0.6M;

            var label = new StringBuilder("! 0 200 200 #Length 1\r\n");
            label.Append("JOURNAL\r\n"); // Disable check for correct media alignment
            label.Append("SPEED 5\r\n"); // Set print speed between 0 and 5, 0 being the slowest
            label.Append("ENCODING UTF-8\r\n");
            label.Append("COUNTRY DENMARK\r\n");
            label.Append("BAR-SENSE\r\n"); // Intruct printer as to means of top-of-form detection
            label.Append("IN-CENTIMETERS\r\n");
            label.Append("LEFT\r\n");

            // T [Font Size] [0] [X] [Y]
            // Header
            label.AppendFormat(LabelCulture, "T 4 0 0.5 1  STOP FOR LEVERING\r\n");
            label.AppendFormat(LabelCulture, "T 5 0 7.5 1 {0}: {1}\r\n", "Dato",
                DateTime.Today.ToString("dd/MM/yyyy"));
            label.AppendFormat(LabelCulture, "L 0.5 1.8 10 1.8 0.03\r\n");


            // Table Header
            label.AppendFormat(LabelCulture, "T 5 0 0.5 2.5 {0}\r\n",
                "A/C No.");
            label.AppendFormat(LabelCulture, "T 5 0 2.4 2.5 {0}\r\n",
                "Name and Address");

            // Table Body Offset is 4.2cm
            var offset = 3M;

            var printedCustomers = new List<String>();

            foreach (var customer in customers)
            {

                System.Diagnostics.Debug.WriteLine(customer.CustomerName1);

                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n",
                                   offset, customer.CustomerAccountNumber);

                if (!String.IsNullOrEmpty(customer.CustomerName1))
                {

                    label.AppendFormat(LabelCulture, "T 5 0 2.4 {0} {1}\r\n",
                                       offset, customer.CustomerName1);
                    offset = offset + margin;
                }

                if (!String.IsNullOrEmpty(customer.Address1))
                {
                    label.AppendFormat(LabelCulture, "T 5 0 2.4 {0} {1}\r\n",
                                       offset, customer.Address1);
                    offset = offset + margin;
                }

                if (!String.IsNullOrEmpty(customer.PostCode))
                {
                    label.AppendFormat(LabelCulture, "T 5 0 2.4 {0} {1} {2}\r\n",
                        offset, customer.PostCode, customer.Address4);
                    offset = offset + margin;
                }

            }

            label.Replace("#Length", (offset * 100).ToString(LabelCulture));
            label.Append("PRINT\r\n");

            return label.ToString();
        }

        /// <summary>
        /// Gets the denmark itinerary report label.
        /// </summary>
        /// <returns>The denmark itinerary report label.</returns>
        /// <param name="runNumber">Run number.</param>
        /// <param name="calls">Calls.</param>
        public static string GetEndOfDayLabel(string runNumber, List<Call> calls, string trailer, string driver,
             List<DriverStock> trailerStockItems, Subscriber subscriber)
        {
            const decimal margin = 0.6M;

            var label = new StringBuilder("! 0 200 200 #Length 1\r\n");
            label.Append("JOURNAL\r\n"); // Disable check for correct media alignment
            label.Append("SPEED 6\r\n"); // Set print speed between 0 and 5, 0 being the slowest
            label.Append("ENCODING UTF-8\r\n");
            label.Append("COUNTRY DENMARK\r\n");
            label.Append("BAR-SENSE\r\n"); // Intruct printer as to means of top-of-form detection
            label.Append("IN-CENTIMETERS\r\n");
            label.Append("LEFT\r\n");

            // T [Font Size] [0] [X] [Y]
            // Header
            label.AppendFormat(LabelCulture, "T 5 0 0.5 0.5  {0}: {1},{2},{3},{4}\r\n", "Afsender",
                subscriber.DisplayName, subscriber.Address, subscriber.PostCode, subscriber.City);

            label.AppendFormat(LabelCulture, "T 4 0 0.5 1.3 {0}\r\n", "Godsdeklaration");
            label.AppendFormat(LabelCulture, "T 5 0 6.6 1.3 {0}: {1}\r\n", "Dato", DateTime.Today.ToString("dd/MM/yyyy"));
            label.AppendFormat(LabelCulture, "L 0.5 2.0 10 2.0 0.03\r\n");
            label.AppendFormat(LabelCulture, "T 5 0 0.5 2.2 {0}: {1}\r\n", "Chauffør", driver);
            label.AppendFormat(LabelCulture, "T 5 0 4.2 2.2 {0}: {1}\r\n", "Reg. Nr.", "");
            label.AppendFormat(LabelCulture, "T 5 0 6.6 2.2 {0}: {1}\r\n", "Tur", runNumber);
            label.AppendFormat(LabelCulture, "T 5 0 0.5 2.8 {0}: {1}\r\n", "Bil", trailer);
            label.AppendFormat(LabelCulture, "L 0.5 3.3 10 3.3 0.03\r\n");

            // Start position to draw the table border
            const decimal start = 3.3M;

            // Table Header
            label.AppendFormat(LabelCulture, "T 5 0 0.5 3.5 {0}\r\n", "Produkt");
            label.AppendFormat(LabelCulture, "T 5 0 2.4 3.5 {0}*\r\n", "Fulde");
            label.AppendFormat(LabelCulture, "T 5 0 3.5 3.5 {0}\r\n", "Vægt Netto kg");
            label.AppendFormat(LabelCulture, "T 5 0 5.7 3.5 {0}\r\n", "Vægt Brutto kg");
            label.AppendFormat(LabelCulture, "T 5 0 8.2 3.5 {0}**\r\n", "Tom");
            label.AppendFormat(LabelCulture, "L 0.5 4.0 10 4.0 0.03\r\n");

            // Table Body Offset is 4.2cm
            var offset = 4.2M;

            var items = trailerStockItems.Where(x => x.HasValue).ToList();

            foreach (var item in items)
            {
                var grossWeight = item.GrossWeight * item.Fulls;
                var netWeight = item.GallonsKilosPerFill * item.Fulls;

                label.AppendFormat(LabelCulture, "LEFT 0.5\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", offset, item.ShortDescription);

                label.AppendFormat(LabelCulture, "RIGHT 3\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 2.4 {0} {1}\r\n", offset, item.Fulls);
                label.AppendFormat(LabelCulture, "RIGHT 5.3\r\n");

                label.AppendFormat(LabelCulture, "T 5 0 3.5 {0} {1:G0}\r\n", offset, item.GallonsKilosPerFill);

                label.AppendFormat(LabelCulture, "RIGHT 7.6\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 5.7 {0} {1:G0}\r\n", offset, item.GrossWeight);

                label.AppendFormat(LabelCulture, "RIGHT 9.4\r\n");
                label.AppendFormat(LabelCulture, "T 5 0 8.2 {0} {1}\r\n", offset, item.Empties);

                offset = offset + margin;
            }

            // Calculate totals
            var totalFulls = items.Sum(x => x.Fulls);
            var totalEmpties = items.Sum(x => x.Empties);

            double totalNetWeight = 0;
            double totalGrossWeight = 0;

            foreach (var item in items.Where(x => x.HasFulls))
            {
                var fulls = item.Fulls + item.FaultyFulls;

                totalNetWeight = Math.Round(totalNetWeight + (fulls * item.GallonsKilosPerFill));
                totalGrossWeight = Math.Round(totalGrossWeight + (fulls * item.GrossWeight));
            }

            // Table Footer
            label.AppendFormat(LabelCulture, "L 0.5 {0} 10 {0} 0.03\r\n", offset);
            offset = offset + margin;

            label.AppendFormat(LabelCulture, "LEFT 0.5\r\n");

            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", offset, "Saldo");

            label.AppendFormat(LabelCulture, "RIGHT 3\r\n");

            label.AppendFormat(LabelCulture, "T 5 0 2.4 {0} {1}*\r\n", offset,
                totalFulls);

            label.AppendFormat(LabelCulture, "RIGHT 5.3\r\n");

            label.AppendFormat(LabelCulture, "T 5 0 3.5 {0} {1:G0}\r\n", offset,
                totalNetWeight);

            label.AppendFormat(LabelCulture, "RIGHT 7.6\r\n");
            label.AppendFormat(LabelCulture, "T 5 0 5.7 {0} {1:G0}\r\n", offset,
                totalGrossWeight);

            label.AppendFormat(LabelCulture, "RIGHT 9.4\r\n");
            label.AppendFormat(LabelCulture, "T 5 0 8.2 {0} {1}**\r\n", offset,
                totalEmpties);

            offset = offset + margin;
            label.AppendFormat(LabelCulture, "L 0.5 {0} 10 {0} 0.03\r\n", offset);

            var finish = offset;

            // Borders for table
            label.AppendFormat(LabelCulture, "L 8.0 {0} 8.0 {1} 0.03\r\n", start, finish);

            // Notes
            label.AppendFormat(LabelCulture, "LEFT 0.5\r\n");
            offset = offset + margin;

            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", offset, "* UN 1965, Carbonhydrid gasblanding, fordråbet, N.O.S., 2.1, (B/D)");

            offset = offset + margin;
            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", offset, "fordråbet, n.o.s., 2.1, (B/D)");

            offset = offset + margin;
            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", offset, "* Gasflasker");

            offset = offset + margin;
            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}\r\n", offset, "** Tom flaske, 2.1");

            offset = offset + margin;
            offset = offset + margin;
            offset = offset + margin;

            label.AppendFormat(LabelCulture, "T 4 0 0.5 {0} {1}\r\n", offset, "Leveringsoversigt");
            label.AppendFormat(LabelCulture, "T 5 0 6.5 {0} {1}: {2}\r\n", offset, "Dato",
                DateTime.Today.ToString("dd/MM/yyyy"));

            offset = offset + margin;
            label.AppendFormat(LabelCulture, "L 0.5 {0} 10 {0} 0.03\r\n", offset);

            offset = offset + margin;
            label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}: {2}\r\n", offset, "Bil", trailer);
            label.AppendFormat(LabelCulture, "T 5 0 4.3 {0} {1}: {2}\r\n", offset, "Reg. Nr.", "");
            label.AppendFormat(LabelCulture, "T 5 0 6.5 {0} {1}: {2}\r\n", offset, "Tur", runNumber);

            offset = offset + margin;
            label.AppendFormat(LabelCulture, "L 0.5 {0} 10 {0} 0.03\r\n", offset);


            offset = offset + margin;

            // Iterate through the Customers
            foreach (var call in calls)
            {
                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}: {2}\r\n", offset, "Order", String.Empty);

                label.AppendFormat(LabelCulture, "T 5 0 4.5 {0} {1} {2}\r\n", offset, call.CustomerName1, call.OnStop.GetValueOrDefault() ? "(STOP)" : "");
                offset = offset + margin;

                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}: {2}\r\n", offset, "Kundenr", call.CustomerAccountNumber);
                label.AppendFormat(LabelCulture, "T 5 0 4.5 {0} {1}\r\n", offset, call.Address1);

                offset = offset + margin;

                label.AppendFormat(LabelCulture, "T 5 0 0.5 {0} {1}: {2}\r\n", offset, "Tlf nr.", String.Empty);
                label.AppendFormat(LabelCulture, "T 5 0 4.5 {0} {1} {2}\r\n", offset, call.PostCode, call.Address4);
                offset = offset + margin;

                if (call.Visited)
                {
                    var resource = !String.IsNullOrWhiteSpace(call.NonDeliveryReason) ? call.NonDeliveryReason : "Besøgte";
                    label.AppendFormat(LabelCulture, "T 5 0 4.5 {0} ** {1} **\r\n", offset, resource);
                    offset = offset + margin;
                }

            }

            label.Replace("#Length", (offset * 90).ToString(LabelCulture));
            label.Append("PRINT\r\n");

            return label.ToString();
        }
    }
}
