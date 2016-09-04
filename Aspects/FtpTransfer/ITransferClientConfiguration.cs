using System;

namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Represents the configuration properties of the FTP transfer site objects represented by <see cref="ITransferClientConfiguration"/>.
    /// Note that the interface inherits from <see cref="ICloneable"/> which should be used by the transfer object to clone the properties internally for multiple use.
    /// </summary>
    /// <seealso cref="ICloneable" />
    public interface ITransferClientConfiguration : ICloneable
    {
        #region Properties
        /// <summary>
        /// URL string of the target.
        /// </summary>
        /// <remarks>This is the URL string of the resource to be downloaded (from) or uploaded (to)</remarks>
        /// <value>The URL of the resource.</value>
        string Link { get; }

        /// <summary>
        /// Gets or sets a value that specifies that an SSL connection should be used.
        /// </summary>
        /// <value>
        ///   <c>true</c> if SSL is enabled; otherwise, <c>false</c>.
        /// </value>
        bool EnableSsl { get; }

        /// <summary>
        /// Gets or sets a value which specifies the data type for file transfers.
        /// </summary>
        bool UseBinary { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the control connection to the FTP server is closed after the request completes.
        /// </summary>
        /// <value>
        ///   <c>true</c> if keep-alive-s are enabled; otherwise, <c>false</c>.
        /// </value>
        bool KeepAlive { get; }

        /// <summary>
        /// The user name part of the credentials if required by the remote site.
        /// </summary>
        /// <remarks>Needed in case the remote site requires user credentials.</remarks>
        /// <value>The user name</value>
        string UserName { get; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        string Password { get; }
        #endregion
    }
}
