namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Specifies the file transfer direction.
    /// </summary>
    public enum TransmissionDirection
    {
        /// <summary>
        /// Unknown, uninitialized direction
        /// </summary>
        None,

        /// <summary>
        /// The file transfer is from the remote target to the intranet consumers.
        /// </summary>
        Inbound,

        /// <summary>
        /// The file transfer is from the intranet producers to the remote target.
        /// </summary>
        Outbound,
    }
}
