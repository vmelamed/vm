using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Abstracts a object that can list download and upload files (e.g. from an FTP site).
    /// </summary>
    [ContractClass(typeof(ITransferFilesContract))]
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

    #region ITransferFile contract binding
    [ContractClassFor(typeof(ITransferFiles))]
    abstract class ITransferFilesContract : ITransferFiles
    {
        public Stream DownloadFile(string name)
        {
            Contract.Requires<ArgumentException>(name != null  &&  name.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(name)+" cannot be null, empty string or consist of whitespace characters only.");

            Contract.Ensures(Contract.Result<Stream>() != null);

            throw new NotImplementedException();
        }

        public Task<Stream> DownloadFileAsync(string name)
        {
            Contract.Requires<ArgumentException>(name != null  &&  name.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(name)+" cannot be null, empty string or consist of whitespace characters only.");

            Contract.Ensures(Contract.Result<Stream>() != null);

            throw new NotImplementedException();
        }

        public Stream ListFiles()
        {
            Contract.Ensures(Contract.Result<Stream>() != null);

            throw new NotImplementedException();
        }

        public Task<Stream> ListFilesAsync()
        {
            Contract.Ensures(Contract.Result<Task<Stream>>() != null);

            throw new NotImplementedException();
        }

        public void UploadFile(Stream stream, string name)
        {
            Contract.Requires<ArgumentNullException>(stream != null, nameof(stream));
            Contract.Requires<ArgumentException>(stream.CanRead);
            Contract.Requires<ArgumentException>(name != null  &&  name.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(name)+" cannot be null, empty string or consist of whitespace characters only.");

            throw new NotImplementedException();
        }

        public Task UploadFileAsync(Stream stream, string name)
        {
            Contract.Requires<ArgumentNullException>(stream != null, nameof(stream));
            Contract.Requires<ArgumentException>(stream.CanRead);
            Contract.Requires<ArgumentException>(name != null  &&  name.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(name)+" cannot be null, empty string or consist of whitespace characters only.");

            throw new NotImplementedException();
        }
    }
    #endregion

}