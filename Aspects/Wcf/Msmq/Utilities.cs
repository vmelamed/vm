using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Messaging;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Msmq
{
    /// <summary>
    /// Class Utilities. Implements several MSMQ related utilities.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Creates transactional queue specified by the URL address.
        /// </summary>
        /// <param name="address">The endpoint's address.</param>
        /// <param name="sendersReceivers">
        /// The domain names of the identities of the senders/receivers who should have the right to send/receive to/from the queue.
        /// </param>
        /// <returns>
        /// The address of the queue or <see langword="null"/> if the operation was not successful.
        /// </returns>
        public static string CreateQueue(
            string address,
            params string[] sendersReceivers)
        {
            Contract.Requires<ArgumentNullException>(address != null, nameof(address));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(address), "The argument \"address\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return CreateTheQueue(address, false, true, sendersReceivers);
        }

        /// <summary>
        /// Creates a queue specified by the URL address.
        /// </summary>
        /// <param name="address">The endpoint's address.</param>
        /// <param name="isTransactional">if set to <see langword="true"/> the queue will be transactional.</param>
        /// <param name="sendersReceivers">The domain names of the identities of the senders/receivers who should have the right to send/receive to/from the queue.</param>
        /// <returns>The address of the queue or <see langword="null"/> if the operation was not successful.</returns>
        public static string CreateQueue(
            string address,
            bool isTransactional,
            params string[] sendersReceivers)
        {
            Contract.Requires<ArgumentNullException>(address != null, nameof(address));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(address), "The argument \"address\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return CreateTheQueue(address, false, isTransactional, sendersReceivers);
        }

        /// <summary>
        /// Creates a transactional dead letter queue for the service specified by the URL address of the queue itself. The actual queue name will be pre-pended with with "dlq/".
        /// </summary>
        /// <param name="address">The endpoint's address.</param>
        /// <param name="sendersReceivers">
        /// The domain names of the identities of the senders/receivers who should have the right to send/receive to/from the queue.
        /// </param>
        /// <returns>
        /// The address of the DLQ queue or <see langword="null"/> if the operation was not successful.
        /// </returns>
        public static string CreateDeadLetterQueue(
            string address,
            params string[] sendersReceivers)
        {
            Contract.Requires<ArgumentNullException>(address != null, nameof(address));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(address), "The argument \"address\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return CreateTheQueue(address, true, true, sendersReceivers);
        }

        /// <summary>
        /// Creates the dead letter queue specified by the URL address of the queue itself. The actual queue name is pre-pended with with "dlq/".
        /// </summary>
        /// <param name="address">The endpoint's address.</param>
        /// <param name="isTransactional">Set to <see langword="true" /> if the queue must be transactional.</param>
        /// <param name="sendersReceivers">The domain names of the identities of the senders/receivers who should have the right to send/receive to/from the queue.</param>
        /// <returns>The address of the DLQ queue or <see langword="null"/> if the operation was not successful.</returns>
        public static string CreateDeadLetterQueue(
            string address,
            bool isTransactional,
            params string[] sendersReceivers)
        {
            Contract.Requires<ArgumentNullException>(address != null, nameof(address));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(address), "The argument \"address\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return CreateTheQueue(address, true, isTransactional, sendersReceivers);
        }

        /// <summary>
        /// Deletes the queue specified by the URL address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns><see langword="true"/> if the operation was successful</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="address"/> is <see langword="null"/>, empty or consists of whitespace characters only.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="address"/> is not a valid address of a service's endpoint with MSMQ transport.
        /// </exception>
        public static bool DeleteQueue(
            string address)
        {
            Contract.Requires<ArgumentNullException>(address != null, nameof(address));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(address), "The argument \"address\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Ensures(Contract.Result<bool>() == true);

            var match     = GuardAddress(address);
            var queuePath = QueuePath(
                                match.Groups["queue"].Value,
                                IsPublic(match.Groups["scope"].Value));

            if (MessageQueue.Exists(queuePath))
            {
                MessageQueue.Delete(queuePath);
                Facilities.Facility.LogWriter.TraceInfo("Deleted queue: {0}", queuePath);
            }

            return true;
        }

        static string CreateTheQueue(
            string address,
            bool isDeadLetterQueue,
            bool isTransactional,
            params string[] sendersReceivers)
        {
            Contract.Requires<ArgumentNullException>(address != null, nameof(address));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(address), "The argument \"address\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            var match     = GuardAddress(address);
            var machine   = match.Groups["machine"].Value;
            var queueName = match.Groups["queue"].Value;
            var isPublic  = IsPublic(match.Groups["scope"].Value);
            var queuePath = QueuePath(queueName, isPublic, isDeadLetterQueue);

            // make sure we are using WindowsPrincipal
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            var user = Thread.CurrentPrincipal.Identity.Name;

            if (!MessageQueue.Exists(queuePath))
                using (var queue = MessageQueue.Create(queuePath, isTransactional))
                {
                    queue.SetPermissions(user, MessageQueueAccessRights.FullControl);                   // the current user always has R/W rights
                    queue.SetPermissions("Administrators", MessageQueueAccessRights.FullControl);       // for easy administration later on
                    if (!isDeadLetterQueue)
                        queue.SetPermissions("ANONYMOUS LOGON", MessageQueueAccessRights.WriteMessage); // allow remote calls

                    var users = new StringBuilder();

                    if (sendersReceivers != null && sendersReceivers.Length > 0)
                        foreach (var u in sendersReceivers)
                        {
                            queue.SetPermissions(u, MessageQueueAccessRights.FullControl);              // add the explicitly named readers/writers
                            users.AppendFormat(CultureInfo.InvariantCulture, ", {0}", u);
                        }

                    Facilities.Facility.LogWriter.TraceInfo(
                        "Created queue {0} and granted full permissions to the Administrators and to {1} {2}.",
                        queuePath,
                        user,
                        users.ToString());
                }

            return QueueUrl(machine, queueName, isPublic, isDeadLetterQueue);
        }

        /// <summary>
        /// Checks if the address is a valid net.msmq endpoint address, parses it into constituent elements and returns the result.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>Match.</returns>
        /// <exception cref="System.ArgumentException">The string does not represent a valid net.msmq address.;address</exception>
        static Match GuardAddress(string address)
        {
            Contract.Requires<ArgumentNullException>(address != null, nameof(address));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(address), "The argument \"address\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Ensures(Contract.Result<Match>() != null);
            Contract.Ensures(Contract.Result<Match>().Success == true);

            // parse the address
            var match = RegularExpression.WcfMsmqService.Match(address);

            if (!match.Success)
                throw new ArgumentException("The string does not represent a valid net.msmq address.", "address");

            return match;
        }

        const string PublicAddress         = "/public";
        const string PrivateAddress        = "/private";
        const string PublicQueuePrefix     = "\\Public$";
        const string PrivateQueuePrefix    = "\\Private$";
        const string DeadLetterQueueSuffix = ".dlq";

        static string QueuePrefix(bool isPublic)
        {
            return isPublic ? PublicQueuePrefix : PrivateQueuePrefix;
        }

        static string QueuePath(
            string queueName,
            bool isPublic,
            bool isDlq = false)
        {
            Contract.Requires<ArgumentNullException>(queueName != null, nameof(queueName));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(queueName), "The argument \"queueName\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            if (isDlq)
                queueName = queueName + DeadLetterQueueSuffix;

            return string.Format(
                            CultureInfo.InvariantCulture,
                            ".{0}\\{1}",
                            QueuePrefix(isPublic),
                            queueName);
        }

        static string QueueUrl(
            string machine,
            string queueName,
            bool isPublic,
            bool isDlq = false)
        {
            Contract.Requires<ArgumentNullException>(machine != null, nameof(machine));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(machine), "The argument \"machine\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(queueName != null, nameof(queueName));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(queueName), "The argument \"queueName\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            if (isDlq)
                queueName = queueName + DeadLetterQueueSuffix;

            return string.Format(
                            CultureInfo.InvariantCulture,
                            "net.msmq://{0}{1}/{2}",
                            isDlq ? "localhost" : machine,
                            isPublic ? PublicAddress : PrivateAddress,
                            queueName);
        }

        static bool IsPublic(
            string scope)
        {
            return string.IsNullOrWhiteSpace(scope)  ||
                   scope.Equals(PublicAddress, StringComparison.OrdinalIgnoreCase);
        }
    }
}
