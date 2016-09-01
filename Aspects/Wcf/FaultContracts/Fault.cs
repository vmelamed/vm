using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using vm.Aspects.Wcf.FaultContracts.Metadata;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// This is the base class for all services faults. Mirrors the Exception class. 
    /// Note that the fields StackTrace and Source are transferred in 
    /// DEBUG mode only.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf")]
    [DebuggerDisplay("{GetType().Name, nq}:: {Message}")]
    [MetadataType(typeof(FaultMetadata))]
    public partial class Fault
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Fault"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Fault()
        {
            HttpStatusCode code;

            HttpStatusCode = FaultToHttpStatusCode.TryGetValue(GetType(), out code)
                                    ? code
                                    : HttpStatusCode.InternalServerError;

            Data = new SortedDictionary<string, string>();
#if DEBUG
            var process = Process.GetCurrentProcess();

            ProcessId   = process.Id;
            ProcessName = process.ProcessName;
            ThreadId    = Thread.CurrentThread.ManagedThreadId;
            MachineName = Environment.MachineName;
            User        = Environment.UserName;
#endif
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the full type name of the fault for easier analysis (incl. deserialization) of the fault on the front end.
        /// IMPORTANT: Any change in the name or namespace of the faults will be potentially breaking changes in the API-s.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "WCF will complain.")]
        [DataMember(Name = "faultType")]
        public string FaultType
        {
            get { return GetType().AssemblyQualifiedName; }
            set { }
        }

        /// <summary>
        /// Gets or sets the handling instance ID.
        /// <c>PostHandlingAction.ThrowNewException</c> causes the exception handling application block to throw a new exception of type 
        /// FaultException{XyzFault} containing the same <c>handlingInstanceID</c> after the entire chain of handlers runs.
        /// This would help to track the fault caught at the client down to the original exception logged by the WCF service.
        /// </summary>
        [DataMember(Name = "handlingInstanceId")]
        public Guid HandlingInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the fault's message
        /// </summary>
        [DataMember(Name = "message")]
        public virtual string Message { get; set; }

        /// <summary>
        /// Gets or sets the text of the messages of the inner exception(s) of the original exception that caused this fault.
        /// </summary>
        [DataMember(Name = "innerExceptionsMessages")]
        public string InnerExceptionsMessages { get; protected set; }

        /// <summary>
        /// Gets or sets the HTTP status code. Used for WebHttpBinding scenarios.
        /// Note that the property is not serialized: for SOAP scenarios it doesn't make sense, for WebHttpBinding it should be set to the response.
        /// The property should be used by an exception handling mechanism to set the adequate HTTP status code.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; protected set; }

        /// <summary>
        /// Gets the HTTP status description of the <see cref="HttpStatusCode"/>.
        /// </summary>
        public string HttpStatusDescription => GetHttpStatusDescription(HttpStatusCode);

        /// <summary>
        /// Appends the text of the related exception's inner exception(s) to the Message property.
        /// </summary>
        /// <remarks>
        /// Note that this method has dummy getter and also the property is not marked with DataMemberAttribute. By default the fault exception handler from 
        /// Enterprise Library will copy the properties of the exception to the fault's properties - property for property matched by name. Here the setter 
        /// will extract recursively the needed textual information from the inner exception(s) and will append it to the property 
        /// <see cref="P:InnerExceptionsMessages"/>.
        /// </remarks>
        public Exception InnerException
        {
            get
            {
                Contract.Ensures(Contract.Result<Exception>() == null);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                var builder = new StringBuilder(value.Message);
                var x = value.InnerException;

                while (x != null)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "\n{0}", x.Message);
                    x = x.InnerException;
                }

                InnerExceptionsMessages = builder.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a collection of key-value pairs that provide additional, user-defined information about the exception.
        /// </summary>
        [DataMember(Name = "data")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "It is a DTO.")]
        public IDictionary<string, string> Data { get; set; }

#if DEBUG
        /// <summary>
        /// Gets or sets the user who experienced the fault.
        /// </summary>
        /// <value>The user login ID.</value>
        [DataMember(Name = "user")]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the name of the machine where the exception happened.
        /// </summary>
        [DataMember(Name = "machineName")]
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the process where the exception happened.
        /// </summary>
        [DataMember(Name = "processName")]
        public string ProcessName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the process where the exception happened.
        /// </summary>
        [DataMember(Name = "processId")]
        public int ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the thread on which the exception happened.
        /// </summary>
        [DataMember(Name = "threadId")]
        public int ThreadId { get; set; }

        /// <summary>
        /// String representation of the frames on the call stack at the time the current exception was thrown.
        /// </summary>
        [DataMember(Name = "stackTrace")]
        public string StackTrace { get; set; }

        /// <summary>
        /// The name of the application or the object that causes the error.
        /// </summary>
        [DataMember(Name = "source")]
        public string Source { get; set; }
#endif
        #endregion

        #region Methods
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => ToString(0);

        /// <summary>
        /// Returns a <see cref="System.String"/> that display the value(s) of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that display the value(s) of this instance.
        /// </returns>
        public string ToString(int indentLevel) => this.DumpString(indentLevel);

        /// <summary>
        /// Gets the HTTP status description corresponding to an HTTP status code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>System.String.</returns>
        public static string GetHttpStatusDescription(
            HttpStatusCode code)
        {
            string description;

            return HttpStatusDescriptions.TryGetValue(code, out description) ? description : null;
        }
        #endregion

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(InnerException == null);
        }
    }
}
