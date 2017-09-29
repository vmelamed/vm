namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
    /// <summary>
    /// Represents the &quot;HTTP basic authentication&quot; validation of user name and password.
    /// </summary>
    public interface IBasicAuthenticate
    {
        /// <summary>
        /// Authenticates the specified user credentials.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// <see langword="true" /> if the credentials were authenticated, <see langword="false" /> otherwise.
        /// </returns>
        bool Authenticate(
            string userName,
            string password);
    }
}
