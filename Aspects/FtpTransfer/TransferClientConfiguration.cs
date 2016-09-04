using System;

namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Class TransferClientConfiguration.
    /// </summary>
    /// <seealso cref="vm.Aspects.FtpTransfer.ITransferClientConfiguration" />
    public class TransferClientConfiguration : ITransferClientConfiguration
    {
        /// <summary>
        /// URL string of the target.
        /// </summary>
        /// <remarks>This is the URL string of the resource to be downloaded (from) or uploaded (to)</remarks>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies that an SSL connection should be used.
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Gets or sets a value which specifies the data type for file transfers.
        /// </summary>
        public bool UseBinary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the control connection to the FTP server is closed after the request completes.
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// The user name part of the credentials if required by the remote site.
        /// </summary>
        /// <remarks>Needed in case the remote site requires user credentials.</remarks>
        public string UserName { get; set; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
            => new TransferClientConfiguration
            {
                Link      = Link,
                EnableSsl = EnableSsl,
                UseBinary = UseBinary,
                KeepAlive = KeepAlive,
                UserName  = UserName,
                Password  = Password,
            };
    }
}
