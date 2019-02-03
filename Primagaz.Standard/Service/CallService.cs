using System;
using System.Linq;
using Primagaz.Standard.Entities;

namespace Primagaz.Standard.Service
{
    public static class CallService
    {
       /// <summary>
       /// Saves the non delivery.
       /// </summary>
       /// <param name="repository">Repository.</param>
       /// <param name="call">Call.</param>
       /// <param name="nonDeliveryReason">Non delivery reason.</param>
        public static void SaveNonDelivery(Repository repository, Call call, NonDeliveryReason nonDeliveryReason)
        {
            var profile = repository.Profiles.First();

            // set the visited status
            call.SetVisited(true);
            call.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var lastCall = repository.Calls.OrderBy(x => x.Sequence).LastOrDefault(x => x.RunNumber == call.RunNumber);

            var sequence = lastCall != null ? lastCall.Sequence + 1 : call.Sequence;

            // set the call sequence to the end
            call.Sequence = sequence;

            // set the non delivery reason
            call.NonDelivery = true;
            call.NonDeliveryReason = nonDeliveryReason.Name;

            var id = String.Format("{0}{1}{2}", profile.SubscriberID,
                                   call.CustomerAccountNumber, call.RunNumber);

            var nonDelivery = repository.NonDeliveries.Find(id);

            if (nonDelivery == null)
            {
                nonDelivery = new NonDelivery
                {
                    Id = id
                };
            }

            var dateModified = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            nonDelivery = new NonDelivery
            {
                CustomerAccountNumber = call.CustomerAccountNumber,
                RunNumber = call.RunNumber,
                DateModified = dateModified,
                NonDeliveryReasonID = nonDeliveryReason.Id,
                OrderNumber = call.OrderNumber,
                SubscriberID = profile.ParentSubscriberID
            };


            // remove the order if it exists
            if (!string.IsNullOrWhiteSpace(call.OrderNumber))
            {
                var order = repository.Orders.Find(call.OrderNumber);

                if (order != null)
                {
                    order.DateModified = dateModified;
                    order.Completed = true;
                    order.NonDelivery = true;
                }

                call.OrderNumber = null;
            }

            repository.NonDeliveries.Add(nonDelivery);
            repository.SaveChanges();
        }
        
    }
}
