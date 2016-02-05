using System.ServiceModel.Channels;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class WebContentTypeMapperDefaultJson. Specifies the default message content format based on the header value.
    /// </summary>
    public class WebContentTypeMapperDefaultJson : WebContentTypeMapper
    {
        /// <summary>
        /// When overridden in a derived class, returns the message format used for a specified content type.
        /// </summary>
        /// <param name="contentType">The content type that indicates the MIME type of data to be interpreted.</param>
        /// <returns>The <see cref="T:System.ServiceModel.Channels.WebContentFormat" /> that specifies the format to which the message content type is mapped.</returns>
        public override WebContentFormat GetMessageFormatForContentType(
            string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return WebContentFormat.Default;

            var match = RegularExpression.ContentType.Match(contentType);

            if (match.Success)
                switch (match.Groups["format"].Value.ToUpperInvariant())
                {
                case "JAVASCRIPT":
                case "JSON":
                case "X-JAVASCRIPT":
                case "X-JSON":
                    return WebContentFormat.Json;

                case "XML":
                    return WebContentFormat.Xml;
                }

            return WebContentFormat.Json;
        }
    }
}
