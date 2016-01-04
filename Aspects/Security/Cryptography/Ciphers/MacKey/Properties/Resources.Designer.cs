﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace vm.Aspects.Security.Cryptography.Ciphers.Utilities.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("vm.Aspects.Security.Cryptography.Ciphers.Utilities.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find a valid certificate with the specified thumb print in the personal certificates store of the current user..
        /// </summary>
        internal static string CannotFindCert {
            get {
                return ResourceManager.GetString("CannotFindCert", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file {0} already exists. Do you want to override it? [Y/N]: .
        /// </summary>
        internal static string FileExist {
            get {
                return ResourceManager.GetString("FileExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file {0} does not exist..
        /// </summary>
        internal static string FileNotExist {
            get {
                return ResourceManager.GetString("FileNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is not a valid command..
        /// </summary>
        internal static string InvalidCommand {
            get {
                return ResourceManager.GetString("InvalidCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The key is not valid..
        /// </summary>
        internal static string InvalidKey {
            get {
                return ResourceManager.GetString("InvalidKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Press any key to exit....
        /// </summary>
        internal static string PressAnyKey {
            get {
                return ResourceManager.GetString("PressAnyKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please, specify a file name..
        /// </summary>
        internal static string SpecifyFileName {
            get {
                return ResourceManager.GetString("SpecifyFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please, specify a key..
        /// </summary>
        internal static string SpecifyKey {
            get {
                return ResourceManager.GetString("SpecifyKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please, spcify the certificate&apos;s thumbprint..
        /// </summary>
        internal static string SpecifyThumbprint {
            get {
                return ResourceManager.GetString("SpecifyThumbprint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Manages certificate encrypted MAC key files for use with the class
        ///vm.Aspects.Security.Cryptography.KeyedHasher.
        ///
        ///Command syntax:
        ///
        ///MacKey &lt;command&gt; &lt;thumbprint&gt; &lt;file path&gt; [&lt;key&gt;]
        ///
        ///&lt;command&gt;   - specifies the operation that needs to be performed:
        ///    create      creates a MAC key and stores it in the specified file.
        ///    export      displays the clear text of the MAC key.
        ///    import      stores the specified key in the specified file.
        ///    help        displays this help. Also: ?, -?, /? or no para [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Usage {
            get {
                return ResourceManager.GetString("Usage", resourceCulture);
            }
        }
    }
}
