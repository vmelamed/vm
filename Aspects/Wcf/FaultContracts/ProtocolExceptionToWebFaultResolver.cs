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
        const string _rexFaultType = @"\s*""faultType""\s*:\s*""(?<type>[^""]+)""";

        static Lazy<Regex> _faultType = new Lazy<Regex>(() => new Regex(_rexFaultType, RegexOptions.Compiled));

        static Regex RexFaultType => _faultType.Value;

        /// <summary>
        /// Resolves protocol exceptions to <see cref="Fault"/>-s.
        /// </summary>
        /// <param name="exception">The exception to resolve.</param>
        /// <param name="expectedFaults">Expected fault types to check for. Can be <see langword="null"/></param>
        /// <param name="serializedFault">The serialized fault string contained in the <paramref name="exception"/>.</param>
        /// <returns>The resolved <see cref="Fault"/> instance or <see langword="null"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Here we handle the specific exception.")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "We need two results here.")]
        public static Fault Resolve(
            ProtocolException exception,
            Type[] expectedFaults,
            out string serializedFault)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            serializedFault = null;

            var wx = exception.InnerException as WebException;

            if (wx == null)
                return null;

            return Resolve(wx, expectedFaults, out serializedFault);
        }

        /// <summary>
        /// Resolves web exceptions to <see cref="Fault" />-s.
        /// </summary>
        /// <param name="exception">The exception to resolve.</param>
        /// <param name="expectedFaults">Expected fault types to check for. Can be <see langword="null"/></param>
        /// <param name="responseText">The serialized fault string contained in the <paramref name="exception" />.</param>
        /// <returns>The resolved <see cref="Fault" /> instance or <see langword="null" />.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Here we handle the specific exception.")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "We need two results here.")]
        public static Fault Resolve(
            WebException exception,
            Type[] expectedFaults,
            out string responseText)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            responseText = null;

            try
            {
                var stream = exception.Response?.GetResponseStream();

                if (stream == null)
                    return null;

                var reader = new StreamReader(stream, true);

                responseText = reader.ReadToEnd();
                stream.Seek(0, SeekOrigin.Begin);

                if (string.IsNullOrWhiteSpace(responseText))
                    return null;

                foreach (var type in expectedFaults)
                    try
                    {
                        var s = new DataContractJsonSerializer(
                                            type,
                                            new DataContractJsonSerializerSettings
                                            {
                                                DateTimeFormat            = new DateTimeFormat("o", CultureInfo.InvariantCulture),
                                                EmitTypeInformation       = EmitTypeInformation.Never,
                                                UseSimpleDictionaryFormat = true,
                                            });

                        return s.ReadObject(stream) as Fault;
                    }
                    catch (SerializationException)
                    {
                    }
                    finally
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                    }

                // try the vm.Aspects.Wcf known faults:
                var matchType = RexFaultType.Match(responseText);

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
