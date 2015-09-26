namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Enum ServiceIdentity defines the types of service identification to the client.
    /// See https://msdn.microsoft.com/en-us/library/ms733130.aspx.
    /// </summary>
    public enum ServiceIdentity
    {
        /// <summary>
        /// Not set - assume the defaults.
        /// </summary>
        None,

        /// <summary>
        /// Use this element with X.509 certificates or Windows accounts.
        /// It compares the DNS name specified in the credential with the value specified in this element.
        /// A DNS check enables you to use certificates with DNS or subject names. 
        /// If a certificate is reissued with the same DNS or subject name, then the identity check is still valid. 
        /// When a certificate is reissued, it gets a new RSA key but retains the same DNS or subject name.
        /// This means that clients do not have to update their identity information about the service. 
        /// </summary>
        Dns,

        /// <summary>
        /// The default when ClientCredentialType is set to Certificate.
        /// The certificate type of identification of the service is the default. 
        /// This restricts authentication to a single certificate based upon its thumbprint value.
        /// This enables stricter authentication because thumbprint values are unique.
        /// This comes with one caveat: If the certificate is reissued with the same Subject name, it also has a new Thumbprint.
        /// Therefore, clients are not able to validate the service unless the new thumbprint is known.
        /// </summary>
        Certificate,

        /// <summary>
        /// The RSA check enables you to specifically restrict authentication to a single certificate based upon its RSA key. 
        /// This enables stricter authentication of a specific RSA key at the expense of the service, which no longer works with existing clients if the RSA key value changes.
        /// </summary>
        Rsa,

        /// <summary>
        /// User principal name (UPN). The default when the ClientCredentialType is set to Windows and the service process is not running under one of the system accounts.
        /// This ensures that the service is running under a specific Windows user account. 
        /// The user account can be either the current logged-on user or the service running under a particular user account.
        /// This setting takes advantage of Windows Kerberos security if the service is running under a domain account within an Active Directory environment.
        /// </summary>
        Upn,

        /// <summary>
        /// Service principal name (SPN). The default when the ClientCredentialType is set to Windows and 
        /// the service process is running under one of the system accounts—LocalService, LocalSystem, or NetworkService.
        /// This ensures that the SPN and the specific Windows account associated with the SPN identify the service.
        /// </summary>
        Spn,
    }
}