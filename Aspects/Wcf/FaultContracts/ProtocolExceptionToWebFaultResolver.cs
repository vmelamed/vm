using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Text.RegularExpressions;
using vm.Aspects.Wcf.Clients;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Contains a utility method for translating <see cref="ProtocolException"/>-s to <see cref="Fault"/>-s.
    /// The utility is useful when calling Web/HTTP services from WCF clients, e.g. based on <see cref="LightClient{TContract}"/>
    /// </summary>
    public static class ProtocolExceptionToWebFaultResolver
    {
        const string _rexFaultType = @",\s*""faultType""\s*:\s*""(?<type>[^""]+)"",";

        static Lazy<Regex> _faultType = new Lazy<Regex>(() => new Regex(_rexFaultType, RegexOptions.Compiled));

        static Regex RexFaultType => _faultType.Value;

        /// <summary>
        /// Resolves protocol exceptions to <see cref="Fault"/>-s.
        /// </summary>
        /// <param name="exception">The exception to resolve.</param>
        /// <param name="serializedFault">The serialized fault string contained in the <paramref name="exception"/>.</param>
        /// <returns>The resolved <see cref="Fault"/> instance or <see langword="null"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Here we handle the specific exception.")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "We need two results here.")]
        public static Fault Resolve(
            ProtocolException exception,
            out string serializedFault)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            serializedFault = null;

            try
            {
                var wx = exception.InnerException as WebException;

                if (wx == null)
                    return null;

                var stream = wx.Response.GetResponseStream();
                var reader = new StreamReader(stream, true);

                serializedFault = reader.ReadToEnd();
                stream.Seek(0, SeekOrigin.Begin);

                var matchType = RexFaultType.Match(serializedFault);

                if (!matchType.Success)
                    return null;

                var faultType = Type.GetType(matchType.Groups["type"].Value, false);

                if (faultType == null)
                    return null;

                var serializer = new DataContractJsonSerializer(
                                            faultType,
                                            new DataContractJsonSerializerSettings
                                            {
                                                DateTimeFormat            = new DateTimeFormat("o", CultureInfo.InvariantCulture),
                                                EmitTypeInformation       = EmitTypeInformation.Never,
                                                UseSimpleDictionaryFormat = true,
                                            });
                return serializer.ReadObject(stream) as Fault;
            }
            catch (SerializationException)
            {
                return null;
            }
        }
    }
}
