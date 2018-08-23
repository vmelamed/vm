﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Using alternative format for the certificate thumbprint.
        /// </summary>
        internal static string AlternativeThumbprintFormat {
            get {
                return ResourceManager.GetString("AlternativeThumbprintFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Using alternative format for the certificate thumbprint and overwriting the key file if it exists.
        /// </summary>
        internal static string AlternativeThumbprintFormatQuiet {
            get {
                return ResourceManager.GetString("AlternativeThumbprintFormatQuiet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find a valid certificate with the specified thumbprint in the personal certificate store of the current user..
        /// </summary>
        internal static string CannotFindCert {
            get {
                return ResourceManager.GetString("CannotFindCert", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file already exists..
        /// </summary>
        internal static string FileAlreadyExists {
            get {
                return ResourceManager.GetString("FileAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generate and certificate-encrypt a symmetric, encryption key and save it in the specified key file.
        /// </summary>
        internal static string HelpTextCreateCertificate {
            get {
                return ResourceManager.GetString("HelpTextCreateCertificate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generate and DPAPI-encrypt (the default) a symmetric, encryption key and save it in the specified key file.
        /// </summary>
        internal static string HelpTextCreateDpapi {
            get {
                return ResourceManager.GetString("HelpTextCreateDpapi", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generate and certificate-encrypt a symmetric, hash-encryption key (HMAC) and save it in the specified key file.
        /// </summary>
        internal static string HelpTextCreateMac {
            get {
                return ResourceManager.GetString("HelpTextCreateMac", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Decrypt and display the certificate-encrypted, symmetric, encryption key from the specified key file as a clear-text, hexadecimal, dash-separated sequence of bytes.
        /// </summary>
        internal static string HelpTextExportCertificate {
            get {
                return ResourceManager.GetString("HelpTextExportCertificate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Decrypt and display the DPAPI-encrypted (the default), symmetric, encryption key from the specified key file as a clear-text, hexadecimal, dash-separated sequence of bytes.
        /// </summary>
        internal static string HelpTextExportDpapi {
            get {
                return ResourceManager.GetString("HelpTextExportDpapi", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Decrypt and display the certificate-encrypted, symmetric, hash-encryption key (HMAC) from the specified key file as a clear-text, hexadecimal, dash-separated sequence of bytes.
        /// </summary>
        internal static string HelpTextExportMac {
            get {
                return ResourceManager.GetString("HelpTextExportMac", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Certificate-encrypt and import the symmetric, encryption key from the command line to the specified key file.
        /// </summary>
        internal static string HelpTextImportCertificate {
            get {
                return ResourceManager.GetString("HelpTextImportCertificate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DPAPI-encrypt (the default) and import the symmetric, encryption key from the command line to the specified key file.
        /// </summary>
        internal static string HelpTextImportDpapi {
            get {
                return ResourceManager.GetString("HelpTextImportDpapi", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Certificate-encrypt and import the symmetric, hash-encryption key (HMAC) from the command line to the specified key file.
        /// </summary>
        internal static string HelpTextImportMac {
            get {
                return ResourceManager.GetString("HelpTextImportMac", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MyApplication.exe.key.
        /// </summary>
        internal static string HelpTextKeyFileName {
            get {
                return ResourceManager.GetString("HelpTextKeyFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The symmetric key is not valid..
        /// </summary>
        internal static string InvalidKey {
            get {
                return ResourceManager.GetString("InvalidKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid certificate thumbprint..
        /// </summary>
        internal static string InvalidThumbprint {
            get {
                return ResourceManager.GetString("InvalidThumbprint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 08-FF-F9-D3-2D-BD-BF-B5-06-20-15-2B-5F-D0-3A-E5-B1-12-F4-0A-B1-3E-3B-EF-9E-7A-17-9C-FD-0A-1D-F6.
        /// </summary>
        internal static string KeyExampleDashes {
            get {
                return ResourceManager.GetString("KeyExampleDashes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 08FFF9D32DBDBFB50620152B5FD03AE5B112F40AB13E3BEF9E7A179CFD0A1DF6.
        /// </summary>
        internal static string KeyExampleNoSeparators {
            get {
                return ResourceManager.GetString("KeyExampleNoSeparators", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 08 FF F9 D3 2D BD BF B5 06 20 15 2B 5F D0 3A E5 B1 12 F4 0A B1 3E 3B EF 9E 7A 17 9C FD 0A 1D F6.
        /// </summary>
        internal static string KeyExampleSpaces {
            get {
                return ResourceManager.GetString("KeyExampleSpaces", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The key file already exists. Do you want to overwrite it? [Y/N]: .
        /// </summary>
        internal static string OverwriteKeyFile {
            get {
                return ResourceManager.GetString("OverwriteKeyFile", resourceCulture);
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
        ///   Looks up a localized string similar to Please, specify the certificate&apos;s thumbprint..
        /// </summary>
        internal static string SpecifyThumbprint {
            get {
                return ResourceManager.GetString("SpecifyThumbprint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 31-89-82-59-8e-4a-c1-43-df-c9-58-96-a8-f7-2b-f0-8c-5d-f9-f4.
        /// </summary>
        internal static string ThumbprintExampleDashes {
            get {
                return ResourceManager.GetString("ThumbprintExampleDashes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 318982598e4ac143dfc95896a8f72bf08c5df9f4.
        /// </summary>
        internal static string ThumbprintExampleNoSeparators {
            get {
                return ResourceManager.GetString("ThumbprintExampleNoSeparators", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 31 89 82 59 8e 4a c1 43 df c9 58 96 a8 f7 2b f0 8c 5d f9 f4.
        /// </summary>
        internal static string ThumbprintExampleSpaces {
            get {
                return ResourceManager.GetString("ThumbprintExampleSpaces", resourceCulture);
            }
        }
    }
}
