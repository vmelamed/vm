using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using vm.Aspects.Exceptions;
using vm.Aspects.Threading;

namespace vm.Aspects.Wcf.FaultContracts
{
    public partial class Fault
    {
        /// <summary>
        /// Gets the fault type corresponding to the given exception type.
        /// </summary>
        /// <param name="exceptionType">The type of exception.</param>
        /// <returns>The corresponding type of fault or <see langword="null"/>.</returns>
        public static Type GetFaultTypeForException(
            Type exceptionType)
        {
            Contract.Requires<ArgumentNullException>(exceptionType != null, nameof(exceptionType));
            Contract.Requires<ArgumentException>(typeof(Exception).IsAssignableFrom(exceptionType), "The argument must be an exception type.");

            Type faultType;

            using (_lock.ReaderLock())
                return _exceptionToFault.TryGetValue(exceptionType, out faultType)
                            ? faultType
                            : null;
        }

        /// <summary>
        /// Gets the exception type corresponding to the given fault type.
        /// </summary>
        /// <param name="faultType">The type of fault.</param>
        /// <returns>The corresponding type of exception or <see langword="null"/>.</returns>
        public static Type GetExceptionTypeForFault(
            Type faultType)
        {
            Contract.Requires<ArgumentNullException>(faultType != null, nameof(faultType));
            Contract.Requires<ArgumentException>(typeof(Fault).IsAssignableFrom(faultType), "The argument must be a fault type.");

            Type exceptionType;

            using (_lock.ReaderLock())
                return _faultToException.TryGetValue(faultType, out exceptionType)
                        ? exceptionType
                        : null;
        }

        /// <summary>
        /// Adds a new fault to exception types mapping.
        /// </summary>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <param name="faultType">Type of the fault.</param>
        /// <param name="exceptionFactory">A factory delegate which should produce an exception object from a fault object.</param>
        /// <param name="faultFactory">
        /// A factory delegate which should produce a fault object from an exception object.
        /// If <see langword="null"/> this method will assume a default factory which will create the fault object and 
        /// will attempt to copy all exception's properties to the fault's properties with the same and type.
        /// Properties that do not have a counterpart, will be added to the <see cref="Data"/> dictionary 
        /// with values converted to string (invoking <see cref="object.ToString()"/>)
        /// </param>
        /// <param name="force">if set to <see langword="true" /> the mapping will be implemented even if the types participate in existing mappings,
        /// otherwise if the types are already mapped to, the method will throw <see cref="InvalidOperationException" />.</param>
        /// <exception cref="InvalidOperationException">The type {exceptionType.Name} is already mapped to {faultType.Name}.
        /// or
        /// The type {faultType.Name} is already mapped to {exceptionType.Name}.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "By design.")]
        public static void AddMappingFaultToException(
            Type exceptionType,
            Type faultType,
            Func<Fault, Exception> exceptionFactory,
            Func<Exception, Fault> faultFactory = null,
            bool force = false)
        {
            Contract.Requires<ArgumentNullException>(exceptionType != null, nameof(exceptionType));
            Contract.Requires<ArgumentNullException>(faultType != null, nameof(faultType));
            Contract.Requires<ArgumentException>(typeof(Exception).IsAssignableFrom(exceptionType), "The argument "+nameof(exceptionType)+" must be an exception type.");
            Contract.Requires<ArgumentException>(typeof(Fault).IsAssignableFrom(faultType), "The argument "+nameof(faultType)+" must be a fault type.");

            using (_lock.UpgradableReaderLock())
            {
                if (!force)
                {
                    Type existing;

                    if (_exceptionToFault.TryGetValue(exceptionType, out existing)  &&  existing != faultType)
                        throw new InvalidOperationException($"The type {exceptionType.Name} is already mapped to {faultType.Name}.");
                    if (_faultToException.TryGetValue(faultType, out existing)  &&  existing != exceptionType)
                        throw new InvalidOperationException($"The type {faultType.Name} is already mapped to {exceptionType.Name}.");
                }

                using (_lock.WriterLock())
                {
                    _exceptionToFault[exceptionType] = faultType;
                    _faultToException[faultType]     = exceptionType;

                    _exceptionToFaultFactories[exceptionType] = faultFactory ?? FaultFactory;
                    _faultToExceptionFactories[faultType]     = exceptionFactory ?? (f => new Exception(f.Message).PopulateData(f.Data));
                }
            }
        }

        /// <summary>
        /// Removes the fault to exception types mapping with the specified types.
        /// </summary>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <param name="faultType">Type of the fault.</param>
        public static void RemoveMappingFaultToException(
            Type exceptionType,
            Type faultType)
        {
            Contract.Requires<ArgumentNullException>(exceptionType != null, nameof(exceptionType));
            Contract.Requires<ArgumentNullException>(faultType != null, nameof(faultType));
            Contract.Requires<ArgumentException>(typeof(Exception).IsAssignableFrom(exceptionType), "The argument "+nameof(exceptionType)+" must be an exception type.");
            Contract.Requires<ArgumentException>(typeof(Fault).IsAssignableFrom(faultType), "The argument "+nameof(faultType)+" must be a fault type.");

            using (_lock.WriterLock())
            {
                _exceptionToFault.Remove(exceptionType);
                _faultToException.Remove(faultType);

                _exceptionToFaultFactories.Remove(exceptionType);
                _faultToExceptionFactories.Remove(faultType);
            }
        }

        /// <summary>
        /// Factory for generating faults out of the corresponding exceptions.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>Fault.</returns>
        public static Fault FaultFactory(
            Exception exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
            Contract.Ensures(Contract.Result<Fault>() != null);

            Type faultType;
            HttpStatusCode httpCode;
            bool found;

            using (_lock.ReaderLock())
            {
                found =_exceptionToFault.TryGetValue(exception.GetType(), out faultType);
                if (!ExceptionToHttpStatusCode.TryGetValue(exception.GetType(), out httpCode))
                    httpCode = ExceptionToHttpStatusCode[typeof(Exception)];
            }

            var exceptionProperties = exception
                                        .GetType()
                                        .GetProperties(BindingFlags.Public|BindingFlags.Instance)
                                        .Where(pi => pi.GetGetMethod() != null);

            if (!found)
            {
                var result = new Fault
                {
                    Message        = exception.Message,
                    HttpStatusCode = httpCode,
                    Data           = exceptionProperties
                                        .ToDictionary(
                                            pi => pi.Name,
                                            pi => pi.GetValue(exception).ToString()),
                };
                result.Data["Exception.Dump"] = exception.DumpString();
                return result;
            };

            Fault fault = (Fault)faultType
                                    .GetConstructor(Type.EmptyTypes)
                                    .Invoke(null)
                                    ;

            fault.Data = new Dictionary<string, string>();

            foreach (var xpi in exceptionProperties)
                try
                {
                    var fpi = faultType.GetProperty(xpi.Name, xpi.PropertyType);

                    if (fpi != null)
                        fpi.SetValue(fault, xpi.GetValue(exception));
                    else
                        fault.Data[$"Exception.{xpi.Name}"] = xpi.GetValue(exception).ToString();
                }
                catch (AmbiguousMatchException)
                {
                }

            if (exception.Data != null  &&  exception.Data.Count > 0)
            {
                fault.Data = new Dictionary<string, string>();

                foreach (DictionaryEntry de in exception.Data)
                    fault.Data[de.Key.ToString()] = de.Value.ToString();
            }

            return fault;
        }

        static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Maps exception types to fault types
        /// </summary>
        static IDictionary<Type, Type> _exceptionToFault = new Dictionary<Type, Type>
        {
            [typeof(Exception)]                             = typeof(Fault),
            [typeof(AggregateException)]                    = typeof(AggregateFault),
            [typeof(ArgumentException)]                     = typeof(ArgumentFault),
            [typeof(ArgumentNullException)]                 = typeof(ArgumentNullFault),
            [typeof(ArgumentValidationException)]           = typeof(ArgumentValidationFault),
            [typeof(BusinessException)]                     = typeof(BusinessFault),
            [typeof(DataException)]                         = typeof(DataFault),
            [typeof(DirectoryNotFoundException)]            = typeof(DirectoryNotFoundFault),
            [typeof(FileNotFoundException)]                 = typeof(FileNotFoundFault),
            [typeof(FormatException)]                       = typeof(FormatFault),
            [typeof(InvalidOperationException)]             = typeof(InvalidOperationFault),
            [typeof(IOException)]                           = typeof(IOFault),
            [typeof(NotImplementedException)]               = typeof(NotImplementedFault),
            [typeof(ObjectNotFoundException)]               = typeof(ObjectNotFoundFault),
            [typeof(ObjectIdentifierNotUniqueException)]    = typeof(ObjectIdentifierNotUniqueFault),
            [typeof(PathTooLongException)]                  = typeof(PathTooLongFault),
            [typeof(RepeatableOperationException)]          = typeof(RepeatableOperationFault),
            [typeof(SerializationException)]                = typeof(SerializationFault),
            [typeof(UnauthorizedAccessException)]           = typeof(UnauthorizedAccessFault),
            [typeof(XmlException)]                          = typeof(XmlFault),
        };

        /// <summary>
        /// Maps fault types to exception types
        /// </summary>
        static IDictionary<Type, Type> _faultToException = new Dictionary<Type, Type>
        {
            [typeof(Fault)]                                 = typeof(Exception),
            [typeof(AggregateFault)]                        = typeof(AggregateException),
            [typeof(ArgumentFault)]                         = typeof(ArgumentException),
            [typeof(ArgumentNullFault)]                     = typeof(ArgumentNullException),
            [typeof(ArgumentValidationFault)]               = typeof(ArgumentValidationException),
            [typeof(BusinessFault)]                         = typeof(BusinessException),
            [typeof(DataFault)]                             = typeof(DataException),
            [typeof(DirectoryNotFoundFault)]                = typeof(DirectoryNotFoundException),
            [typeof(FileNotFoundFault)]                     = typeof(FileNotFoundException),
            [typeof(FormatFault)]                           = typeof(FormatException),
            [typeof(InvalidOperationFault)]                 = typeof(InvalidOperationException),
            [typeof(IOFault)]                               = typeof(IOException),
            [typeof(NotImplementedFault)]                   = typeof(NotImplementedException),
            [typeof(ObjectNotFoundFault)]                   = typeof(ObjectNotFoundException),
            [typeof(ObjectIdentifierNotUniqueFault)]        = typeof(ObjectIdentifierNotUniqueException),
            [typeof(PathTooLongFault)]                      = typeof(PathTooLongException),
            [typeof(RepeatableOperationFault)]              = typeof(RepeatableOperationException),
            [typeof(SerializationFault)]                    = typeof(SerializationException),
            [typeof(UnauthorizedAccessFault)]               = typeof(UnauthorizedAccessException),
            [typeof(XmlFault)]                              = typeof(XmlException),
        };

        static IDictionary<Type, Func<Exception, Fault>> _exceptionToFaultFactories = new Dictionary<Type, Func<Exception, Fault>>
        {
            [typeof(Exception)]                             = FaultFactory,
            [typeof(AggregateException)]                    = FaultFactory,
            [typeof(ArgumentException)]                     = FaultFactory,
            [typeof(ArgumentNullException)]                 = FaultFactory,
            [typeof(ArgumentValidationException)]           = FaultFactory,
            [typeof(BusinessException)]                     = FaultFactory,
            [typeof(DataException)]                         = FaultFactory,
            [typeof(DirectoryNotFoundException)]            = FaultFactory,
            [typeof(FileNotFoundException)]                 = FaultFactory,
            [typeof(FormatException)]                       = FaultFactory,
            [typeof(InvalidOperationException)]             = FaultFactory,
            [typeof(IOException)]                           = FaultFactory,
            [typeof(NotImplementedException)]               = FaultFactory,
            [typeof(ObjectNotFoundException)]               = FaultFactory,
            [typeof(ObjectIdentifierNotUniqueException)]    = FaultFactory,
            [typeof(PathTooLongException)]                  = FaultFactory,
            [typeof(RepeatableOperationException)]          = FaultFactory,
            [typeof(SerializationException)]                = FaultFactory,
            [typeof(UnauthorizedAccessException)]           = FaultFactory,
            [typeof(XmlException)]                          = FaultFactory,
        };


        static IDictionary<Type, Func<Fault, Exception>> _faultToExceptionFactories = new Dictionary<Type, Func<Fault, Exception>>
        {
            [typeof(Fault)]                                 = f => new Exception(f.Message).PopulateData(f.Data),
            [typeof(AggregateFault)]                        = f => new AggregateException(f.Message).PopulateData(f.Data),
            [typeof(ArgumentFault)]                         = f =>
                                                            {
                                                                var x = new ArgumentException(f.Message, ((ArgumentFault)f).ParamName).PopulateData(f.Data);
                                                                x.Data["InnerExceptionMessages"] = f.InnerExceptionsMessages;
                                                                return x;
                                                            },
            [typeof(ArgumentNullFault)]                     = f => new ArgumentNullException(((ArgumentFault)f).ParamName, f.Message).PopulateData(f.Data),
            [typeof(ArgumentValidationFault)]               = f => new ArgumentValidationException(((ArgumentValidationFault)f).ValidationResults, ((ArgumentValidationFault)f).ParamName).PopulateData(f.Data),
            [typeof(BusinessFault)]                         = f => new BusinessException(f.Message).PopulateData(f.Data),
            [typeof(DataFault)]                             = f => new DataException(f.Message).PopulateData(f.Data),
            [typeof(DirectoryNotFoundFault)]                = f => new DirectoryNotFoundException(f.Message).PopulateData(f.Data),
            [typeof(FileNotFoundFault)]                     = f => new FileNotFoundException(f.Message, ((FileNotFoundFault)f).FileName).PopulateData(f.Data),
            [typeof(FormatFault)]                           = f => new FormatException(f.Message).PopulateData(f.Data),
            [typeof(InvalidOperationFault)]                 = f => new InvalidOperationException(f.Message).PopulateData(f.Data),
            [typeof(IOFault)]                               = f => new IOException(f.Message).PopulateData(f.Data),
            [typeof(NotImplementedFault)]                   = f => new NotImplementedException(f.Message).PopulateData(f.Data),
            [typeof(ObjectNotFoundFault)]                   = f =>
                                                            {
                                                                var x = new ObjectNotFoundException(((ObjectNotFoundFault)f).ObjectIdentifier,null,f.Message).PopulateData(f.Data);
                                                                x.Data["ObjectType"] = ((ObjectNotFoundFault)f).ObjectType;
                                                                return x;
                                                            },
            [typeof(ObjectIdentifierNotUniqueFault)]        = f =>
                                                            {
                                                                var x = new ObjectIdentifierNotUniqueException(((ObjectIdentifierNotUniqueFault)f).ObjectIdentifier,null,f.Message).PopulateData(f.Data);
                                                                x.Data["ObjectType"] = ((ObjectNotFoundFault)f).ObjectType;
                                                                return x;
                                                            },
            [typeof(PathTooLongFault)]                      = f => new PathTooLongException(f.Message).PopulateData(f.Data),
            [typeof(RepeatableOperationFault)]              = f => new RepeatableOperationException(f.Message).PopulateData(f.Data),
            [typeof(SerializationFault)]                    = f => new SerializationException(f.Message).PopulateData(f.Data),
            [typeof(UnauthorizedAccessFault)]               = f => new UnauthorizedAccessException(f.Message).PopulateData(f.Data),
            [typeof(XmlFault)]                              = f => new XmlException(f.Message,null,((XmlFault)f).LineNumber,((XmlFault)f).LinePosition).PopulateData(f.Data),
        };

        /// <summary>
        /// Maps fault type to HTTP status code
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "No, really, it is read only.")]
        public static readonly IReadOnlyDictionary<Type, HttpStatusCode> FaultToHttpStatusCode = new ReadOnlyDictionary<Type, HttpStatusCode>(
            new Dictionary<Type, HttpStatusCode>
            {
                [typeof(Fault)]                             = HttpStatusCode.InternalServerError,
                [typeof(AggregateFault)]                    = HttpStatusCode.InternalServerError,
                [typeof(ArgumentFault)]                     = HttpStatusCode.BadRequest,
                [typeof(ArgumentNullFault)]                 = HttpStatusCode.BadRequest,
                [typeof(ArgumentValidationFault)]           = HttpStatusCode.BadRequest,
                [typeof(BusinessFault)]                     = HttpStatusCode.BadRequest,
                [typeof(DataFault)]                         = HttpStatusCode.InternalServerError,
                [typeof(DbUpdateFault)]                     = HttpStatusCode.InternalServerError,
                [typeof(DirectoryNotFoundFault)]            = HttpStatusCode.NotFound,
                [typeof(FileNotFoundFault)]                 = HttpStatusCode.NotFound,
                [typeof(FormatFault)]                       = HttpStatusCode.InternalServerError,
                [typeof(InvalidOperationFault)]             = HttpStatusCode.Conflict,
                [typeof(IOFault)]                           = HttpStatusCode.InternalServerError,
                [typeof(NotImplementedFault)]               = HttpStatusCode.NotImplemented,
                [typeof(ObjectNotFoundFault)]               = HttpStatusCode.NotFound,
                [typeof(ObjectIdentifierNotUniqueFault)]    = HttpStatusCode.BadRequest,
                [typeof(PathTooLongFault)]                  = HttpStatusCode.BadRequest,
                [typeof(RepeatableOperationFault)]          = HttpStatusCode.GatewayTimeout,
                [typeof(SerializationFault)]                = HttpStatusCode.BadRequest,
                [typeof(UnauthorizedAccessFault)]           = HttpStatusCode.Unauthorized,
                [typeof(XmlFault)]                          = HttpStatusCode.InternalServerError,
            });

        /// <summary>
        /// Maps exception type to HTTP status code
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "No, really, it is read only.")]
        public static readonly IReadOnlyDictionary<Type, HttpStatusCode> ExceptionToHttpStatusCode = new ReadOnlyDictionary<Type, HttpStatusCode>(
            new Dictionary<Type, HttpStatusCode>
            {
                [typeof(Exception)]                         = HttpStatusCode.InternalServerError,
                [typeof(AggregateException)]                = HttpStatusCode.InternalServerError,
                [typeof(ArgumentException)]                 = HttpStatusCode.BadRequest,
                [typeof(ArgumentNullException)]             = HttpStatusCode.BadRequest,
                [typeof(ArgumentValidationException)]       = HttpStatusCode.BadRequest,
                [typeof(BusinessException)]                 = HttpStatusCode.BadRequest,
                [typeof(DataException)]                     = HttpStatusCode.InternalServerError,
                [typeof(DirectoryNotFoundException)]        = HttpStatusCode.NotFound,
                [typeof(FileNotFoundException)]             = HttpStatusCode.NotFound,
                [typeof(FormatException)]                   = HttpStatusCode.InternalServerError,
                [typeof(InvalidOperationException)]         = HttpStatusCode.Conflict,
                [typeof(IOException)]                       = HttpStatusCode.InternalServerError,
                [typeof(NotImplementedException)]           = HttpStatusCode.NotImplemented,
                [typeof(ObjectNotFoundException)]           = HttpStatusCode.NotFound,
                [typeof(ObjectIdentifierNotUniqueException)]= HttpStatusCode.BadRequest,
                [typeof(PathTooLongException)]              = HttpStatusCode.BadRequest,
                [typeof(RepeatableOperationException)]      = HttpStatusCode.GatewayTimeout,
                [typeof(SerializationException)]            = HttpStatusCode.BadRequest,
                [typeof(UnauthorizedAccessException)]       = HttpStatusCode.Unauthorized,
                [typeof(ValidationException)]               = HttpStatusCode.BadRequest,
                [typeof(XmlException)]                      = HttpStatusCode.InternalServerError,
            });

        /// <summary>
        /// Maps HTTP status codes to a string descriptions.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "No, really, it is read only.")]
        public static readonly IReadOnlyDictionary<HttpStatusCode, string> HttpStatusDescriptions = new ReadOnlyDictionary<HttpStatusCode, string>(
            new Dictionary<HttpStatusCode, string>
            {
                // 1×× Informational
                [HttpStatusCode.Continue]                     = "Continue",
                [HttpStatusCode.SwitchingProtocols]           = "Switching Protocols",
                [(HttpStatusCode)102]                         = "Processing",
                [(HttpStatusCode)103]                         = "Checkpoint",

                // 2×× Success
                [HttpStatusCode.OK]                           = "OK",
                [HttpStatusCode.Created]                      = "Created",
                [HttpStatusCode.Accepted]                     = "Accepted",
                [HttpStatusCode.NonAuthoritativeInformation]  = "Non-authoritative Information",
                [HttpStatusCode.NoContent]                    = "No Content",
                [HttpStatusCode.ResetContent]                 = "Reset Content",
                [HttpStatusCode.PartialContent]               = "Partial Content",
                [(HttpStatusCode)207]                         = "Multi-Status",
                [(HttpStatusCode)208]                         = "Already Reported",
                [(HttpStatusCode)226]                         = "IM Used",

                // 3×× Redirection
                [HttpStatusCode.MultipleChoices]              = "Multiple Choices",
                [HttpStatusCode.MovedPermanently]             = "Moved Permanently",
                [HttpStatusCode.Found]                        = "Found",
                [HttpStatusCode.SeeOther]                     = "See Other",
                [HttpStatusCode.NotModified]                  = "Not Modified",
                [HttpStatusCode.UseProxy]                     = "Use Proxy",
                [HttpStatusCode.TemporaryRedirect]            = "Temporary Redirect",
                [(HttpStatusCode)308]                         = "Permanent Redirect",

                // 4×× Client Error
                [HttpStatusCode.BadRequest]                   = "Bad Request",
                [HttpStatusCode.Unauthorized]                 = "Unauthorized",
                [HttpStatusCode.PaymentRequired]              = "Payment Required",
                [HttpStatusCode.Forbidden]                    = "Forbidden",
                [HttpStatusCode.NotFound]                     = "Not Found",
                [HttpStatusCode.MethodNotAllowed]             = "Method Not Allowed",
                [HttpStatusCode.NotAcceptable]                = "Not Acceptable",
                [HttpStatusCode.ProxyAuthenticationRequired]  = "Proxy Authentication Required",
                [HttpStatusCode.RequestTimeout]               = "Request Timeout",
                [HttpStatusCode.Conflict]                     = "Conflict",
                [HttpStatusCode.Gone]                         = "Gone",
                [HttpStatusCode.LengthRequired]               = "Length Required",
                [HttpStatusCode.PreconditionFailed]           = "Precondition Failed",
                [HttpStatusCode.RequestEntityTooLarge]        = "Payload Too Large",
                [HttpStatusCode.RequestUriTooLong]            = "Request-URI Too Long",
                [HttpStatusCode.UnsupportedMediaType]         = "Unsupported Media Type",
                [HttpStatusCode.RequestedRangeNotSatisfiable] = "Requested Range Not Satisfiable",
                [HttpStatusCode.ExpectationFailed]            = "Expectation Failed",
                [(HttpStatusCode)418]                         = "I'm a teapot",
                [(HttpStatusCode)419]                         = "Authentication timeout",
                [(HttpStatusCode)421]                         = "Misdirected Request",
                [(HttpStatusCode)422]                         = "Unprocessable Entity",
                [(HttpStatusCode)423]                         = "Locked",
                [(HttpStatusCode)424]                         = "Failed Dependency",
                [HttpStatusCode.UpgradeRequired]              = "Upgrade Required",
                [(HttpStatusCode)428]                         = "Precondition Required",
                [(HttpStatusCode)429]                         = "Too Many Requests",
                [(HttpStatusCode)431]                         = "Request Header Fields Too Large",
                [(HttpStatusCode)451]                         = "Unavailable For Legal Reasons",
                [(HttpStatusCode)499]                         = "Client Closed Request",

                // 5×× Server Error
                [HttpStatusCode.InternalServerError]          = "Internal Server Error",
                [HttpStatusCode.NotImplemented]               = "Not Implemented",
                [HttpStatusCode.BadGateway]                   = "Bad Gateway",
                [HttpStatusCode.ServiceUnavailable]           = "Service Unavailable",
                [HttpStatusCode.GatewayTimeout]               = "Gateway Timeout",
                [HttpStatusCode.HttpVersionNotSupported]      = "HTTP Version Not Supported",
                [(HttpStatusCode)506]                         = "Variant Also Negotiates",
                [(HttpStatusCode)507]                         = "Insufficient Storage",
                [(HttpStatusCode)508]                         = "Loop Detected",
                [(HttpStatusCode)510]                         = "Not Extended",
                [(HttpStatusCode)511]                         = "Network Authentication Required",
                [(HttpStatusCode)599]                         = "Network Connect Timeout Error",
            });
    }
}
