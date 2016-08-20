using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using vm.Aspects.Exceptions;

namespace vm.Aspects.Wcf.FaultContracts
{
    public partial class Fault
    {
        /// <summary>
        /// Maps exception types to fault types
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "No, really, it is read only.")]
        public static readonly IReadOnlyDictionary<Type, Type> ExceptionToFault = new ReadOnlyDictionary<Type, Type>(
            new Dictionary<Type, Type>
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
            });

        /// <summary>
        /// Maps fault types to exception types
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "No, really, it is read only.")]
        public static readonly IReadOnlyDictionary<Type, Type> FaultToException = new ReadOnlyDictionary<Type, Type>(
            new Dictionary<Type, Type>
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
            });

        /// <summary>
        /// Maps fault type to HTTP status code
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "No, really, it is read only.")]
        public static readonly IReadOnlyDictionary<Type, HttpStatusCode> FaultToHttpStatusCode = new ReadOnlyDictionary<Type, HttpStatusCode>(
            new Dictionary<Type, HttpStatusCode>
            {
                [typeof(Fault)]                                 = HttpStatusCode.InternalServerError,
                [typeof(AggregateFault)]                        = HttpStatusCode.InternalServerError,
                [typeof(ArgumentFault)]                         = HttpStatusCode.BadRequest,
                [typeof(ArgumentNullFault)]                     = HttpStatusCode.BadRequest,
                [typeof(ArgumentValidationFault)]               = HttpStatusCode.BadRequest,
                [typeof(BusinessFault)]                         = HttpStatusCode.BadRequest,
                [typeof(DataFault)]                             = HttpStatusCode.InternalServerError,
                [typeof(DbUpdateFault)]                         = HttpStatusCode.InternalServerError,
                [typeof(DirectoryNotFoundFault)]                = HttpStatusCode.NotFound,
                [typeof(FileNotFoundFault)]                     = HttpStatusCode.NotFound,
                [typeof(FormatFault)]                           = HttpStatusCode.InternalServerError,
                [typeof(InvalidOperationFault)]                 = HttpStatusCode.Conflict,
                [typeof(IOFault)]                               = HttpStatusCode.InternalServerError,
                [typeof(NotImplementedFault)]                   = HttpStatusCode.NotImplemented,
                [typeof(ObjectNotFoundFault)]                   = HttpStatusCode.NotFound,
                [typeof(ObjectIdentifierNotUniqueFault)]        = HttpStatusCode.BadRequest,
                [typeof(PathTooLongFault)]                      = HttpStatusCode.BadRequest,
                [typeof(RepeatableOperationFault)]              = HttpStatusCode.GatewayTimeout,
                [typeof(SerializationFault)]                    = HttpStatusCode.BadRequest,
                [typeof(UnauthorizedAccessFault)]               = HttpStatusCode.Unauthorized,
                [typeof(XmlFault)]                              = HttpStatusCode.InternalServerError,
            });

        /// <summary>
        /// Maps exception type to HTTP status code
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "No, really, it is read only.")]
        public static readonly IReadOnlyDictionary<Type, HttpStatusCode> ExceptionToHttpStatusCode = new ReadOnlyDictionary<Type, HttpStatusCode>(
            new Dictionary<Type, HttpStatusCode>
            {
                [typeof(Exception)]                             = HttpStatusCode.InternalServerError,
                [typeof(AggregateException)]                    = HttpStatusCode.InternalServerError,
                [typeof(ArgumentException)]                     = HttpStatusCode.BadRequest,
                [typeof(ArgumentNullException)]                 = HttpStatusCode.BadRequest,
                [typeof(ArgumentValidationException)]           = HttpStatusCode.BadRequest,
                [typeof(BusinessException)]                     = HttpStatusCode.BadRequest,
                [typeof(DataException)]                         = HttpStatusCode.InternalServerError,
                [typeof(DirectoryNotFoundException)]            = HttpStatusCode.NotFound,
                [typeof(FileNotFoundException)]                 = HttpStatusCode.NotFound,
                [typeof(FormatException)]                       = HttpStatusCode.InternalServerError,
                [typeof(InvalidOperationException)]             = HttpStatusCode.Conflict,
                [typeof(IOException)]                           = HttpStatusCode.InternalServerError,
                [typeof(NotImplementedException)]               = HttpStatusCode.NotImplemented,
                [typeof(ObjectNotFoundException)]               = HttpStatusCode.NotFound,
                [typeof(ObjectIdentifierNotUniqueException)]    = HttpStatusCode.BadRequest,
                [typeof(PathTooLongException)]                  = HttpStatusCode.BadRequest,
                [typeof(RepeatableOperationException)]          = HttpStatusCode.GatewayTimeout,
                [typeof(SerializationException)]                = HttpStatusCode.BadRequest,
                [typeof(UnauthorizedAccessException)]           = HttpStatusCode.Unauthorized,
                [typeof(ValidationException)]                   = HttpStatusCode.BadRequest,
                [typeof(XmlException)]                          = HttpStatusCode.InternalServerError,
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
