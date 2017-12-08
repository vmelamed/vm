using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Threading;
using System.Xml;
using vm.Aspects.Diagnostics.ExternalMetadata;
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
        /// <returns>The corresponding type of fault or <see langword="null"/> if cannot find a match.</returns>
        public static Type GetFaultTypeForException(
            Type exceptionType)
        {
            if (exceptionType == null)
                throw new ArgumentNullException(nameof(exceptionType));
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
                throw new ArgumentException("The argument must be an exception type.");

            using (_lock.ReaderLock())
                return _exceptionToFault.TryGetValue(exceptionType, out var faultType)
                            ? faultType
                            : null;
        }

        /// <summary>
        /// Gets the exception type corresponding to the given fault type.
        /// </summary>
        /// <param name="faultType">The type of fault.</param>
        /// <returns>The corresponding type of exception or <see langword="null"/> if cannot find a match.</returns>
        public static Type GetExceptionTypeForFault(
            Type faultType)
        {
            if (faultType == null)
                throw new ArgumentNullException(nameof(faultType));
            if (!typeof(Fault).IsAssignableFrom(faultType))
                throw new ArgumentException("The argument must be a fault type.");

            using (_lock.ReaderLock())
                return _faultToException.TryGetValue(faultType, out var exceptionType)
                        ? exceptionType
                        : null;
        }

        /// <summary>
        /// Adds a new fault to exception types mapping.
        /// </summary>
        /// <typeparam name="TFault">The type of the fault.</typeparam>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exceptionFactory">A factory delegate which should produce an exception object from a given fault object.</param>
        /// <param name="httpStatusCode">The HTTP status code mapped to the fault-exception pair. The default is InternalServerError (500).</param>
        /// <param name="faultFactory">A factory delegate which should produce a fault object from an exception object.
        /// If <see langword="null" /> this method will assume a default factory which will create the fault object and
        /// will attempt to copy all exception's properties to the fault's properties with the same and type.
        /// Properties that do not have a counterpart, will be added to the <see cref="Data" /> dictionary
        /// with values converted to string (invoking <see cref="object.ToString()" />)</param>
        /// <param name="force">if set to <see langword="true" /> the mapping will be implemented even if the types participate in existing mappings,
        /// otherwise if the types are already mapped to, the method will throw <see cref="InvalidOperationException" />.</param>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        /// <exception cref="InvalidOperationException">The type {exceptionType.Name} is already mapped to {faultType.Name}.
        /// or
        /// The type {faultType.Name} is already mapped to {exceptionType.Name}.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "No choice here.")]
        public static void AddMappingFaultToException<TFault, TException>(
            Func<Fault, TException> exceptionFactory,
            HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError,
            Func<Exception, TFault> faultFactory = null,
            bool force = false)
            where TFault : Fault
            where TException : Exception
        {
            using (_lock.UpgradableReaderLock())
            {
                if (!force)
                {
                    if (_exceptionToFault.TryGetValue(typeof(TException), out var existing) && existing != typeof(TFault))
                        throw new InvalidOperationException($"The type {typeof(TException).Name} is already mapped to {existing.Name}.");

                    if (_faultToException.TryGetValue(typeof(TFault), out existing)  &&  existing != typeof(TException))
                        throw new InvalidOperationException($"The type {typeof(TFault).Name} is already mapped to {existing.Name}.");
                }

                using (_lock.WriterLock())
                {
                    _exceptionToFault[typeof(TException)]         = typeof(TFault);
                    _faultToException[typeof(TFault)]             = typeof(TException);
                    ExceptionToHttpStatusCode[typeof(TException)] = httpStatusCode;
                    FaultToHttpStatusCode[typeof(TFault)]         = httpStatusCode;

                    if (faultFactory != null)
                        _exceptionToFaultFactories[typeof(TException)] = faultFactory;
                    else
                        _exceptionToFaultFactories[typeof(TException)] = FaultFactory<TException>;

                    if (exceptionFactory != null)
                        _faultToExceptionFactories[typeof(TFault)] = exceptionFactory;
                    else
                        _faultToExceptionFactories[typeof(TFault)] = f => new Exception(f.Message).PopulateData(f.Data);
                }
            }
        }

        /// <summary>
        /// Removes the fault to exception types mapping with the specified types.
        /// </summary>
        /// <param name="faultType">Type of the fault.</param>
        /// <param name="exceptionType">Type of the exception.</param>
        public static void RemoveMappingFaultToException(
            Type faultType,
            Type exceptionType)
        {
            if (exceptionType == null)
                throw new ArgumentNullException(nameof(exceptionType));
            if (faultType == null)
                throw new ArgumentNullException(nameof(faultType));
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
                throw new ArgumentException("The argument "+nameof(exceptionType)+" must be an exception type.");
            if (!typeof(Fault).IsAssignableFrom(faultType))
                throw new ArgumentException("The argument "+nameof(faultType)+" must be a fault type.");

            using (_lock.WriterLock())
            {
                _exceptionToFault.Remove(exceptionType);
                _faultToException.Remove(faultType);

                _exceptionToFaultFactories.Remove(exceptionType);
                _faultToExceptionFactories.Remove(faultType);
            }
        }

        /// <summary>
        /// Removes the fault to exception types mapping with the specified types.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <typeparam name="TFault">The type of the fault.</typeparam>
        public static void RemoveMappingFaultToException<TException, TFault>()
            => RemoveMappingFaultToException(typeof(TException), typeof(TFault));

        // ----------------------------------------------------------------------

        /// <summary>
        /// Tries to get the <paramref name="exceptionType"/> exception to fault factory.
        /// </summary>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <returns>The Exception to Fault factory delegate or <see langword="null"/> if cannot find a match.</returns>
        public static Func<Exception, Fault> GetExceptionToFaultFactory(
            Type exceptionType)
        {
            if (exceptionType == null)
                throw new ArgumentNullException(nameof(exceptionType));
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
                throw new ArgumentException("The argument must be Exception or descendant type.");

            if (_exceptionToFaultFactories.TryGetValue(exceptionType, out var factory))
                return factory;
            else
                return null;
        }

        /// <summary>
        /// Tries to get the exception <typeparamref name="TException"/> to fault factory.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <returns>Func{Exception, Fault} factory delegate or <see langword="null"/> if cannot find a match.</returns>
        public static Func<Exception, Fault> GetExceptionToFaultFactory<TException>()
            => GetExceptionToFaultFactory(typeof(TException));

        /// <summary>
        /// Tries to get the <paramref name="faultType"/> fault to exception factory.
        /// </summary>
        /// <param name="faultType">Type of the fault.</param>
        /// <returns>The Fault to Exception factory delegate or <see langword="null"/> if cannot find a match.</returns>
        public static Func<Fault, Exception> GetFaultToExceptionFactory(
            Type faultType)
        {
            if (faultType == null)
                throw new ArgumentNullException(nameof(faultType));
            if (!typeof(Fault).IsAssignableFrom(faultType))
                throw new ArgumentException("The argument must be Exception or descendant type.");

            if (_faultToExceptionFactories.TryGetValue(faultType, out var factory))
                return factory;
            else
                return null;
        }

        /// <summary>
        /// Tries to get the <typeparamref name="TFault"/> fault to exception factory.
        /// </summary>
        /// <typeparam name="TFault">The type of the fault.</typeparam>
        /// <returns>The Fault to Exception factory delegate or <see langword="null"/> if cannot find a match.</returns>
        public static Func<Fault, Exception> GetFaultToExceptionFactory<TFault>()
            => GetFaultToExceptionFactory(typeof(TFault));

        // --------------------------------------------------------------------

        /// <summary>
        /// Factory for generating faults out of the corresponding exceptions.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>Fault.</returns>
        public static Fault FaultFactory<TException>(
            Exception exception)
            where TException : Exception
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var exceptionProperties = exception
                                        .GetType()
                                        .GetProperties(BindingFlags.Public|BindingFlags.Instance)
                                        .Where(pi => pi.GetGetMethod() != null  &&
                                                     pi.GetIndexParameters().Count() == 0  &&
                                                     pi.Name != nameof(Exception.StackTrace))   // do not copy the stack trace to the fault
                                        ;

            var isFound = false;
            Type faultType;

            using (_lock.ReaderLock())
                isFound =_exceptionToFault.TryGetValue(typeof(TException), out faultType);

            Fault fault;

            if (!isFound)
            {
                fault = new Fault
                {
                    Message        = exception.Message,
                    HttpStatusCode = HttpStatusCode.InternalServerError,
                    Data           = exceptionProperties
                                        .Where(pi => pi.Name != nameof(Exception.Message))
                                        .ToDictionary(
                                            pi => pi.Name,
                                            pi => pi.GetValue(exception)?.ToString()),
                };

                fault.Data[$"{nameof(Exception)}.Dump"] = exception.DumpString(0, typeof(ExceptionDumpNoStackMetadata));
                return fault;
            };

            fault = (Fault)faultType
                            .GetConstructor(Type.EmptyTypes)
                            .Invoke(null);

            foreach (var xpi in exceptionProperties)
                if (xpi.Name!=nameof(Exception.Data)  &&  xpi.GetIndexParameters().Count() == 0)
                    try
                    {
                        var value = xpi.GetValue(exception);

                        if (value == null)
                            continue;

                        var fpi = faultType.GetProperty(xpi.Name, BindingFlags.Public|BindingFlags.Instance, null, xpi.PropertyType, Type.EmptyTypes, null);

                        if (fpi != null  &&  fpi.CanWrite)
                            fpi.SetValue(fault, value);
                        else
                            fault.Data[$"{nameof(Exception)}.{xpi.Name}"] = value.ToString();
                    }
                    catch (AmbiguousMatchException)
                    {
                    }

            return fault.PopulateFaultData(exception.Data);
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
            [typeof(AuthenticationException)]               = typeof(AuthenticationFault),
            [typeof(InvalidObjectException)]                = typeof(InvalidObjectFault),
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
            [typeof(AuthenticationFault)]                   = typeof(AuthenticationException),
            [typeof(InvalidObjectFault)]                    = typeof(InvalidObjectException),
        };

        /// <summary>
        /// Checks that the passed in exception argument if of type <typeparamref name="TException"/> or its descendant.
        /// </summary>
        /// <typeparam name="TException">The actual type of the exception.</typeparam>
        /// <param name="exception">The exception to be checked.</param>
        public static void CheckFault<TException>(Exception exception)
            where TException : Exception
        {
            if (exception is TException)
                return;

            throw new ArgumentException($"The actual type of the exception must be {typeof(TException).Name}.", nameof(exception));
        }

        static IDictionary<Type, Func<Exception, Fault>> _exceptionToFaultFactories = new Dictionary<Type, Func<Exception, Fault>>
        {
            [typeof(Exception)]                             = FaultFactory<Exception>,
            [typeof(AggregateException)]                    = FaultFactory<AggregateException>,
            [typeof(ArgumentException)]                     = FaultFactory<ArgumentException>,
            [typeof(ArgumentNullException)]                 = FaultFactory<ArgumentNullException>,
            [typeof(ArgumentValidationException)]           = FaultFactory<ArgumentValidationException>,
            [typeof(BusinessException)]                     = FaultFactory<BusinessException>,
            [typeof(DataException)]                         = FaultFactory<DataException>,
            [typeof(DirectoryNotFoundException)]            = FaultFactory<DirectoryNotFoundException>,
            [typeof(FileNotFoundException)]                 = FaultFactory<FileNotFoundException>,
            [typeof(FormatException)]                       = FaultFactory<FormatException>,
            [typeof(InvalidOperationException)]             = FaultFactory<InvalidOperationException>,
            [typeof(IOException)]                           = FaultFactory<IOException>,
            [typeof(NotImplementedException)]               = FaultFactory<NotImplementedException>,
            [typeof(ObjectNotFoundException)]               = FaultFactory<ObjectNotFoundException>,
            [typeof(ObjectIdentifierNotUniqueException)]    = FaultFactory<ObjectIdentifierNotUniqueException>,
            [typeof(PathTooLongException)]                  = FaultFactory<PathTooLongException>,
            [typeof(RepeatableOperationException)]          = FaultFactory<RepeatableOperationException>,
            [typeof(SerializationException)]                = FaultFactory<SerializationException>,
            [typeof(UnauthorizedAccessException)]           = FaultFactory<UnauthorizedAccessException>,
            [typeof(XmlException)]                          = FaultFactory<XmlException>,
            [typeof(AuthenticationException)]               = FaultFactory<AuthenticationException>,
            [typeof(InvalidObjectException)]                = x =>
                                                                {
                                                                    var f = FaultFactory<InvalidObjectException>(x);
                                                                    var elements = new List<ValidationFaultElement>();

                                                                    ((InvalidObjectException)x).ValidationResults.CopyTo(elements);
                                                                    ((InvalidObjectFault)f).Details = elements;

                                                                    return f;
                                                                },
        };

        /// <summary>
        /// Checks that the passed in fault argument is actually of type <typeparamref name="TFault"/> or its descendant.
        /// </summary>
        /// <typeparam name="TFault">The actual type of the fault.</typeparam>
        /// <param name="fault">The fault to be checked.</param>
        public static void CheckFault<TFault>(Fault fault)
            where TFault : Fault
        {
            if (fault is TFault)
                return;

            throw new ArgumentException($"The actual type of the fault must be {typeof(TFault).Name}.", nameof(fault));
        }

        static IDictionary<Type, Func<Fault, Exception>> _faultToExceptionFactories = new Dictionary<Type, Func<Fault, Exception>>
        {
            [typeof(Fault)]                                 = f => { CheckFault<Fault>(f); return new Exception(f.Message).PopulateData(f); },
            [typeof(AggregateFault)]                        = f => { CheckFault<Fault>(f); return new AggregateException(f.Message).PopulateData(f); },
            [typeof(ArgumentFault)]                         = f =>
                                                            {
                                                                CheckFault<ArgumentFault>(f);

                                                                var x = new ArgumentException(f.Message, ((ArgumentFault)f).ParamName);
                                                                x.Data["InnerExceptionMessages"] = f.InnerExceptionsMessages;
                                                                return x.PopulateData(f);
                                                            },
            [typeof(ArgumentNullFault)]                     = f => { CheckFault<Fault>(f); return new ArgumentNullException(((ArgumentNullFault)f).ParamName, f.Message).PopulateData(f); },
            [typeof(ArgumentValidationFault)]               = f => { CheckFault<Fault>(f); return new ArgumentValidationException(((ArgumentValidationFault)f).ValidationResults, ((ArgumentValidationFault)f).ParamName).PopulateData(f); },
            [typeof(BusinessFault)]                         = f => { CheckFault<Fault>(f); return new BusinessException(f.Message).PopulateData(f); },
            [typeof(DataFault)]                             = f => { CheckFault<Fault>(f); return new DataException(f.Message).PopulateData(f); },
            [typeof(DirectoryNotFoundFault)]                = f => { CheckFault<Fault>(f); return new DirectoryNotFoundException(f.Message).PopulateData(f); },
            [typeof(FileNotFoundFault)]                     = f => { CheckFault<Fault>(f); return new FileNotFoundException(f.Message, ((FileNotFoundFault)f).FileName).PopulateData(f); },
            [typeof(FormatFault)]                           = f => { CheckFault<Fault>(f); return new FormatException(f.Message).PopulateData(f); },
            [typeof(InvalidOperationFault)]                 = f => { CheckFault<Fault>(f); return new InvalidOperationException(f.Message).PopulateData(f); },
            [typeof(IOFault)]                               = f => { CheckFault<Fault>(f); return new IOException(f.Message).PopulateData(f); },
            [typeof(NotImplementedFault)]                   = f => { CheckFault<Fault>(f); return new NotImplementedException(f.Message).PopulateData(f); },
            [typeof(ObjectNotFoundFault)]                   = f =>
                                                            {
                                                                CheckFault<ObjectNotFoundFault>(f);

                                                                var x = new ObjectNotFoundException(((ObjectNotFoundFault)f).ObjectIdentifier, null, f.Message);
                                                                x.Data["ObjectType"] = ((ObjectNotFoundFault)f).ObjectType;
                                                                return x.PopulateData(f);
                                                            },
            [typeof(ObjectIdentifierNotUniqueFault)]        = f =>
                                                            {
                                                                CheckFault<Fault>(f);

                                                                var x = new ObjectIdentifierNotUniqueException(((ObjectIdentifierNotUniqueFault)f).ObjectIdentifier, null, f.Message);
                                                                x.Data["ObjectType"] = ((ObjectNotFoundFault)f).ObjectType;
                                                                return x.PopulateData(f);
                                                            },
            [typeof(PathTooLongFault)]                      = f => { CheckFault<Fault>(f); return new PathTooLongException(f.Message).PopulateData(f); },
            [typeof(RepeatableOperationFault)]              = f => { CheckFault<Fault>(f); return new RepeatableOperationException(f.Message).PopulateData(f); },
            [typeof(SerializationFault)]                    = f => { CheckFault<Fault>(f); return new SerializationException(f.Message).PopulateData(f); },
            [typeof(UnauthorizedAccessFault)]               = f => { CheckFault<Fault>(f); return new UnauthorizedAccessException(f.Message).PopulateData(f); },
            [typeof(XmlFault)]                              = f => { CheckFault<Fault>(f); return new XmlException(f.Message, null, ((XmlFault)f).LineNumber, ((XmlFault)f).LinePosition).PopulateData(f); },
            [typeof(AuthenticationFault)]                   = f => { CheckFault<Fault>(f); return new AuthenticationException(f.Message).PopulateData(f); },
            [typeof(InvalidObjectFault)]                    = f =>
                                                            {
                                                                CheckFault<Fault>(f);

                                                                var results = new ValidationResults();
                                                                var vresults = new List<ValidationResult>();

                                                                ((InvalidObjectFault)f).Details.CopyTo(vresults);
                                                                results.AddAllResults(vresults);

                                                                return new InvalidObjectException(results, f.Message, null).PopulateData(f);
                                                            }
        };

        /// <summary>
        /// Maps fault type to HTTP status code
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "No, really, it is read only.")]
        public static readonly IDictionary<Type, HttpStatusCode> FaultToHttpStatusCode = new Dictionary<Type, HttpStatusCode>
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
            [typeof(AuthenticationFault)]               = HttpStatusCode.Unauthorized,
            [typeof(InvalidObjectFault)]                = HttpStatusCode.BadRequest,
        };

        /// <summary>
        /// Maps exception type to HTTP status code
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "No, really, it is read only.")]
        public static readonly IDictionary<Type, HttpStatusCode> ExceptionToHttpStatusCode = new Dictionary<Type, HttpStatusCode>
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
            [typeof(InvalidObjectException)]            = HttpStatusCode.BadRequest,
            [typeof(XmlException)]                      = HttpStatusCode.InternalServerError,
            [typeof(AuthenticationException)]           = HttpStatusCode.Unauthorized,
        };

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
