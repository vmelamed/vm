namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// The user credentials if required by a remote target site.
    /// </summary>
    public class FtpUserCredentials
    {
        /// <summary>
        /// The user name part of the credentials if required by the remote site.
        /// </summary>
        /// <remarks>Needed in case the remote site requires user credentials.</remarks>
        /// <value>The user name</value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password { get; set; }
    }
}
