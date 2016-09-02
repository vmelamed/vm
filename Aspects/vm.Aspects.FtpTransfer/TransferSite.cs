using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Represents an FTP site.
    /// </summary>
    /// <seealso cref="ITransferFile" />
    public class TransferSite : ITransferFile
    {
        #region Properties
        /// <summary>
        /// URL string of the target.
        /// </summary>
        /// <remarks>This is the URL string of the resource to be downloaded (from) or uploaded (to)</remarks>
        /// <value>The URL of the resource.</value>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the enum value of the property TransmissionDirection.
        /// </summary>
        public TransmissionDirection TransmissionDirection { get; set; }

        /// <summary>
        /// Gets or sets the status code from the last transfer operation.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the status description from the FTP operation.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Gets or sets the name of the list parser for this (inbound) site which will parse the text list of available streams for download to a list of objects.
        /// </summary>
        /// <value>
        /// The name of the list parser.
        /// </value>
        public string ListParser { get; set; }

        /// <summary>
        /// Gets or sets the credentials which might be needed at the target site.
        /// </summary>
        /// <value>
        /// The credentials.
        /// </value>
        public FtpUserCredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets the data by which to find the corresponding client certificate in the Windows certificate stores.
        /// </summary>
        /// <value>
        /// The certificate data.
        /// </value>
        public CertificateData ClientCertificate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the control connection to the FTP server is closed after the request completes.
        /// </summary>
        /// <value>
        ///   <c>true</c> if keep-alive-s are enabled; otherwise, <c>false</c>.
        /// </value>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies that an SSL connection should be used.
        /// </summary>
        /// <value>
        ///   <c>true</c> if SSL is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Gets or sets a value which specifies the data type for file transfers.
        /// </summary>
        public bool UseBinary { get; set; }
        #endregion

        /// <summary>
        /// Callback to validate a server certificate.
        /// </summary>
        /// <remarks>
        /// The default implementation here allows for self-signed certificate within the intranet and 
        /// the local computer otherwise it fails if the server certificate is not kosher.
        /// </remarks>
        bool ServerCertificateValidation(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors) ||
                sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
            {
                // allow self-signed certificates
                Zone z = Zone.CreateFromUrl(((WebRequest)sender).RequestUri.ToString());

                if (z.SecurityZone == SecurityZone.Intranet ||
                    z.SecurityZone == SecurityZone.MyComputer)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Lists the files available for receiving at the target (something like dir).
        /// </summary>
        /// <returns>The stream that will receive the files list text. The interpretation of the list text is up to the consumer.</returns>
        public Stream ListFiles()
        {
            var request = PrepareFileRequest(WebRequestMethods.Ftp.ListDirectoryDetails, null);
            var response = request.GetResponse();

            return response.GetResponseStream();
        }

        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Task{Stream}.</returns>
        public Stream DownloadFile(string name)
        {
            var request = PrepareFileRequest(WebRequestMethods.Ftp.DownloadFile, name);
            var response = request.GetResponse();

            return response.GetResponseStream();
        }

        /// <summary>
        /// Uploads a file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="name">The name.</param>
        /// <returns>Task.</returns>
        public void UploadFile(Stream stream, string name)
        {
            var request = PrepareFileRequest(WebRequestMethods.Ftp.UploadFile, name);
            var targetStream = request.GetRequestStream();

            stream.CopyTo(targetStream);
        }

        // ---------------------------------------------------------------------------

        /// <summary>
        /// Asynchronously lists the files available for receiving at the target (something like dir).
        /// </summary>
        /// <returns>The stream that will receive the files list text. The interpretation of the list text is up to the consumer.</returns>
        public async Task<Stream> ListFilesAsync()
        {
            var request = PrepareFileRequest(WebRequestMethods.Ftp.ListDirectoryDetails, null);
            var response = await request.GetResponseAsync();

            return response.GetResponseStream();
        }

        /// <summary>
        /// Asynchronously downloads a file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Task{Stream}.</returns>
        public async Task<Stream> DownloadFileAsync(string name)
        {
            var request = PrepareFileRequest(WebRequestMethods.Ftp.DownloadFile, name);
            var response = await request.GetResponseAsync();

            return response.GetResponseStream();
        }

        /// <summary>
        /// Asynchronously uploads a file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="name">The name.</param>
        /// <returns>Task.</returns>
        public async Task UploadFileAsync(Stream stream, string name)
        {
            var request = PrepareFileRequest(WebRequestMethods.Ftp.UploadFile, name);
            var targetStream = await request.GetRequestStreamAsync();

            await stream.CopyToAsync(targetStream);
        }


        /// <summary>
        /// Prepares the FTP request. After calling this the only thing that will be needed will be the FTP method.
        /// </summary>
        /// <param name="method">The FTP method to perform. Use one of the constants in <see cref="WebRequestMethods.Ftp"/></param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>
        /// An <see cref="FtpWebRequest"/> instance.
        /// </returns>
        FtpWebRequest PrepareFileRequest(
            string method,
            string fileName)
        {
            var fileUrl = Link;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                if (!fileUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase) &&
                    !fileUrl.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
                    fileUrl += '/';

                if (fileName.StartsWith("/", StringComparison.OrdinalIgnoreCase) ||
                    fileName.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
                    fileName = Regex.Replace(fileName, @"^[\/\\]+", string.Empty);

                fileUrl += fileName;
            }

            // prepare the request and begin getting the request stream 
            var request = WebRequest.Create(new Uri(fileUrl)) as FtpWebRequest;

            Debug.Assert(request != null);

            request.Method    = method;
            request.UseBinary = UseBinary;
            request.KeepAlive = KeepAlive;
            request.EnableSsl = EnableSsl;

            if (EnableSsl)
                ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidation;

            if (Credentials != null &&
                !string.IsNullOrWhiteSpace(Credentials.UserName))
                request.Credentials = new NetworkCredential(
                                            Credentials.UserName,
                                            Credentials.Password);

            var clientCertificate = ClientCertificate?.Certificate;

            if (clientCertificate != null)
                request.ClientCertificates.Add(clientCertificate);

            return request;
        }
    }
}
