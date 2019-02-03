using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Primagaz.Standard.Entities;

namespace Primagaz.Standard.Service
{
    public static class RunService
    {
        /// <summary>
        /// Find all runs
        /// </summary>
        /// <returns>The by subscriber.</returns>
        public static List<Run> FindAll(Repository repository)
        {
            return repository.Runs.Where(x => !x.Closed).OrderByDescending(x => x.DeliveryDate).ToList();
        }

        /// <summary>
        /// Instantiate the run
        /// </summary>
        public static Run CreateRun(Repository repository)
        {
            Run run = null;

            var profile = repository.Profiles.First();
            var device = repository.MobileDevices.First();

            var runNumber = CreateRunNumber(device);

            // get the existing run by run number
            var existingRun = repository.Runs.Find(runNumber);

            // if it already exists
            while (existingRun != null)
            {
                // increment run number
                runNumber = CreateRunNumber(device);
                existingRun = repository.Runs.Find(runNumber);
            }

            run = new Run
            {
                Name = runNumber, // default name to run number
                RunNumber = runNumber,
                SubscriberID = profile.ParentSubscriberID,
                ChildSubscriberID = profile.SubscriberID,
                DriverRun = true,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            repository.Runs.Add(run);
            repository.SaveChanges();

            return run;
        }

        private static string CreateRunNumber(MobileDevice device)
        {
            device.RunNumber++;

            var runPrefix = new StringBuilder(device.Id.ToString().PadLeft(5, '0'));
            runPrefix.Append(device.RunNumber.ToString().PadLeft(3, '0'));

            return runPrefix.ToString();
        }

        /// <summary>
        /// Move calls
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="runNumber">Run number.</param>
        /// <param name="fromPosition">From position.</param>
        /// <param name="toPosition">To position.</param>
        public static void MoveCalls(Repository repository, string runNumber, int fromPosition, int toPosition)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var calls = repository.Calls.Where(x => x.RunNumber == runNumber);

            var callA = calls.ElementAt(fromPosition);
            var callB = calls.ElementAt(toPosition);

            var seqA = callA.Sequence;
            var seqB = callB.Sequence;

            callA.Sequence = seqB;
            callB.Sequence = seqA;

            callA.Timestamp = timestamp;
            callB.Timestamp = timestamp;

            repository.SaveChanges();

        }

        /// <summary>
        /// Remove call
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="call">Call.</param>
        public static void RemoveCall(Repository repository, Call call)
        {
            call.Removed = true;
            repository.SaveChanges();
        }

        /// <summary>
        /// Updates the name of the run.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="run">Run.</param>
        /// <param name="name">Name.</param>
        public static void UpdateRunName(Repository repository, Run run, string name)
        {

            run.Name = name;
            run.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            repository.SaveChanges();
        }

        /// <summary>
        /// Closes the run.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="run">Run.</param>
        public static void CloseRun(Repository repository, Run run)
        {

            run.Closed = true;
            run.EndTime = DateTimeOffset.UtcNow;
            run.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            repository.SaveChanges();

        }
    }
}
