using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class DIBehaviorExtensionElement. Represents a configuration element that contains DI endpoint behavior.
    /// </summary>
    public class DIBehaviorExtensionElement : BehaviorExtensionElement
    {
        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        /// <returns>The type of behavior.</returns>
        public override Type BehaviorType => typeof(DIEndpointBehavior);

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>The behavior extension.</returns>
        protected override object CreateBehavior() => new DIEndpointBehavior(ResolveName);

        /// <summary>
        /// Gets or sets the name of the resolve.
        /// </summary>
        [ConfigurationProperty("resolveName", DefaultValue=null, IsKey=false, IsRequired=false)]
        public string ResolveName
        {
            get { return (string)this["resolveName"]; }
            set { this["resolveName"] = value; }
        }
    }
}
