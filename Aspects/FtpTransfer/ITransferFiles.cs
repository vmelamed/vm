using System.IO;
using System.Threading.Tasks;

namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Abstracts a object that can list download and upload files (e.g. from an FTP site).
    /// </summary>
    public interface ITransferFiles
    {
        /// <summary>
        /// Lists the files available for receiving at the target (something like dir or ls).
        /// </summary>
        /// <returns>The stream that will receive the files list text. The interpretation of the list text is up to the consumer.</returns>
        Stream ListFiles();

        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The downloaded stream.</returns>
        Stream DownloadFile(string name);

        /// <summary>
        /// Uploads a file.
        /// </summary>
        /// <param name="stream">The uploaded stream.</param>
        /// <param name="name">The name of the stream at the remote site.</param>
        void UploadFile(Stream stream, string name);

        // -----------------------------------------

        /// <summary>
        /// Asynchronously lists the files available for receiving at the target (something like dir or ls).
        /// </summary>
        /// <returns>The stream that will receive the files list text. The interpretation of the list text is up to the consumer.</returns>
        Task<Stream> ListFilesAsync();

        /// <summary>
        /// Asynchronously downloads a file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The downloading Task{Stream}.</returns>
        Task<Stream> DownloadFileAsync(string name);

        /// <summary>
        /// Asynchronously uploads a file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="name">The name.</param>
        /// <returns>The uploading task.</returns>
        Task UploadFileAsync(Stream stream, string name);
    }
}