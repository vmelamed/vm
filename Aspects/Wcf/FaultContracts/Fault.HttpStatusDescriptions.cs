using System.Collections.Generic;
using System.Net;

namespace vm.Aspects.Wcf.FaultContracts
{
    public partial class Fault
    {
        static readonly IDictionary<HttpStatusCode, string> _httpStatusDescriptions = new Dictionary<HttpStatusCode, string>
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
        };
    }
}
