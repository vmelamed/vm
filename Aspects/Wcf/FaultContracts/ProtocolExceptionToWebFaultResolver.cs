﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Text;
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
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Here we handle the specific exception.")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "We need two results here.")]
        public static Fault Resolve(
            ProtocolException exception,
            Type[] expectedFaults,
            out string serializedFault)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            serializedFault = null;

            if (!(exception.InnerException is WebException wx))
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
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Here we handle the specific exception.")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "We need two results here.")]
        public static Fault Resolve(
            WebException exception,
            Type[] expectedFaults,
            out string responseText)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            responseText = null;

            var stream = exception.Response?.GetResponseStream();

            if (stream == null)
                return null;

            var reader = new StreamReader(stream, Encoding.UTF8);

            responseText = reader.ReadToEnd();
            stream.Seek(0, SeekOrigin.Begin);

            if (responseText.IsNullOrWhiteSpace())
                return null;

            // try the vm.Aspects.Wcf known faults:
            var matchType = RexFaultType.Match(responseText);
            Fault fault = null;

            if (matchType.Success)
            {
                var faultType = Type.GetType(matchType.Groups["type"].Value, false);

                if (faultType != null)
                {
                    fault = DeserializeFault(faultType, stream);
                    if (fault != null)
                        return fault;
                }
            }

            // try the expected faults
            if (expectedFaults != null)
                foreach (var type in expectedFaults)
                {
                    fault = DeserializeFault(type, stream);
                    if (fault != null)
                        return fault;
                }

            return null;
        }

        static Fault DeserializeFault(Type type, Stream stream)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(
                                        type,
                                        new DataContractJsonSerializerSettings
                                        {
                                            DateTimeFormat      = new DateTimeFormat("o", CultureInfo.InvariantCulture),
                                            EmitTypeInformation = EmitTypeInformation.Never,
                                        });

                return serializer.ReadObject(stream) as Fault;
            }
            catch (SerializationException)
            {
                // swallow it and try the next one
                return null;
            }
            finally
            {
                stream?.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
