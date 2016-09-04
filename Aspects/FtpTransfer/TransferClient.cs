using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
    /// <seealso cref="ITransferFiles" />
    public class TransferClient : ITransferFiles
    {
        #region Properties
        /// <summary>
        /// Gets the FTP configuration.
        /// </summary>
        ITransferClientConfiguration Configuration { get; }

        /// <summary>
        /// Gets the client certificate identifying this owner to the FTP server.
        /// </summary>
        X509Certificate2 ClientCertificate { get; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferClient"/> class.
        /// </summary>
        /// <param name="configuration">The FTP client configuration.</param>
        /// <param name="clientCertificate">The FTP client certificate.</param>
        public TransferClient(
            ITransferClientConfiguration configuration,
            X509Certificate2 clientCertificate = null)
        {
            Contract.Requires<ArgumentNullException>(configuration != null, nameof(configuration));

            Configuration     = (ITransferClientConfiguration)configuration.Clone();
            ClientCertificate = clientCertificate;
        }

        // ---------------------------------------------------------------------------

        /// <summary>
        /// Lists the files available for receiving at the target (something like dir).
        /// </summary>
        /// <returns>The stream that will receive the files list text. The interpretation of the list text is up to the consumer.</returns>
        public Stream ListFiles()
        {
            var request  = PrepareFileRequest(WebRequestMethods.Ftp.ListDirectoryDetails, null);
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
            var request  = PrepareFileRequest(WebRequestMethods.Ftp.DownloadFile, name);
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

            using (var requestStream = request.GetRequestStream())
                stream.CopyTo(requestStream);

            using (var response = request.GetResponse())
                response.Close();
        }

        // ---------------------------------------------------------------------------

        /// <summary>
        /// Asynchronously lists the files available for receiving at the target (something like dir).
        /// </summary>
        /// <returns>The stream that will receive the files list text. The interpretation of the list text is up to the consumer.</returns>
        public async Task<Stream> ListFilesAsync()
        {
            var request  = PrepareFileRequest(WebRequestMethods.Ftp.ListDirectoryDetails, null);
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
            var request  = PrepareFileRequest(WebRequestMethods.Ftp.DownloadFile, name);
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

            using (var requestStream = await request.GetRequestStreamAsync())
                await stream.CopyToAsync(requestStream);

            using (var response = await request.GetResponseAsync())
                response.Close();
        }

        // ---------------------------------------------------------------------------

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
            var fileUrl = Configuration.Link;

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
            request.UseBinary = Configuration.UseBinary;
            request.KeepAlive = Configuration.KeepAlive;
            request.EnableSsl = Configuration.EnableSsl;

            if (Configuration.EnableSsl)
                ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidation;

            if (!string.IsNullOrWhiteSpace(Configuration.UserName))
                request.Credentials = new NetworkCredential(
                                            Configuration.UserName,
                                            Configuration.Password);

            if (ClientCertificate != null)
                request.ClientCertificates.Add(ClientCertificate);

            return request;
        }

        /// <summary>
        /// Callback to validate a server certificate.
        /// </summary>
        /// <remarks>
        /// The default implementation here allows for self-signed certificate within the intranet and 
        /// the local computer otherwise it fails if the server certificate is not kosher.
        /// </remarks>
        bool ServerCertificateValidation(
            object sender,
            X509Certificate serverCertificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors) ||
                sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
            {
                // allow self-signed certificates
                var z = Zone.CreateFromUrl(((WebRequest)sender).RequestUri.ToString());

                if (z.SecurityZone == SecurityZone.Intranet ||
                    z.SecurityZone == SecurityZone.MyComputer)
                    return true;
            }

            return false;
        }
    }
}
