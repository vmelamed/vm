namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Interface IConfigurationProvider represents a configuration source (from file, DB, etc.) which provides a set of key-value pairs - the settings.
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Gets the value of a setting with the specified setting name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>The value of the setting. May be null</returns>
        string this[string settingName] { get; }
    }
}
