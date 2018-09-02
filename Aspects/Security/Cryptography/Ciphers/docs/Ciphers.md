# Ciphers

The Ciphers library contains a number of classes which perform complete cryptographic tasks over pieces of data like streams, byte arrays, and XML documents. Each of the classes implements some cryptographic scheme and produces or consumes crypto-packages. The crypto-packages contain the encrypted data, as well as information for initializing the ciphers like initialization vectors, hash salts, encrypted symmetric keys, etc.

The symmetric encryption keys are encrypted and decrypted with respectively public and private asymmetric keys stored in certificates. Certificates are also used for encryption of hashes (signatures).

Encrypting a stream and producing a crypto-package from it is as simple as:

```csharp
ICipher cipher = GetCipher();

// ...
cipher.Encrypt(dataStream, encryptedStream);

// or
cipher.Decrypt(encryptedStream, dataStream);
```

## Documentation, Samples, and Tests

This document is intended to be more of a general description of the packages and their components. For detailed information, please refer to the source code all programming elements are well XML comment-documented.
Good usage samples of the ciphers are the unit tests. Most of the unit test methods are implemented in base generic test classes which the actual test classes inherit from. Usually the concrete test classes are also good examples for instantiation of the cipher classes, and the methods in the generics – for using the interfaces and the extensions.

Before running the unit tests make sure to execute the batch file **`CreateCertificates.cmd`** which creates a few self-signed certificates in the current user's, private, certificate store and are used by the unit tests.

## Packages, Assemblies, and Namespaces

The ciphers are distributed in three different packages in order to limit the dependency on non-`.NET Standard` *extension packages* like `System.Security.Cryptography.Xml` and `System.Security.Cryptography.Xml.ProtectedData`.

### **`vm.Aspects.Security.Cryptography.Ciphers`** - the Root Package

 This is the biggest of the packages. It contains the definitions of all of the non-XML interfaces and all but two of the classes (the DPAPI dependent classes - see below) from the namespace by the same name. This package depends only on **`.NET Standard`**.

### **`vm.Aspects.Security.Cryptography.Ciphers.Xml`**

This package contains the types that can be used to perform cryptographic tasks on XML documents: encryption, hashing and signing XML. These types are defined in the namespace by the same name and depend on:
* `.NET Standard`
* `System.Security.Cryptography.Xml`
* `vm.Aspects.Security.Cryptography.Ciphers`
> _I posted a question on StackOverflow: [why `System.Security.Cryptography.Xml` is not part of .NET Standard](https://stackoverflow.com/questions/52130774/system-security-cryptography-xml-not-part-of-net-standard), but if and when Microsoft adds it as a standard API, this package should be merged to `vm.Aspects.Security.Cryptography.Ciphers`._

### **`vm.Aspects.Security.Cryptography.Ciphers.ProtectedData`**

This package uses the Windows-specific Data Protection API (DPAPI) for encrypting data and XML documents. Hence it depends on:
* `.NET Standard`
* `System.Security.Cryptography.Xml`
* `System.Security.Cryptography.ProtectedData`
* `vm.Aspects.Security.Cryptography.Ciphers`

The package `System.Security.Cryptography.ProtectedData` is the extension that provides access to the DPAPI and should probably always stay as an extension package.

> _In my opinion, this package should be rarely used anyway but is there for completeness._

# Supporting Interfaces and Classes

In order for the ciphers to do their job in providing a complete end-to-end cryptographic services they must accept or implement some strategies for implementing cross-cutting tasks like:
* Choose symmetric encryption algorithm or hash algorithm.
* Generate, export and import encryption keys.
* Store and retrieve the encryption keys to external media.
* Implement an addressing scheme for the external media.

## Algorithms

**The classes from the Ciphers packages do not implement any cryptographic algorithms.** They leverage the cryptographic algorithms implemented by the .NET cryptographic service providers from the namespace `System.Security.Cryptography`.

All concrete algorithms are identified by `string`-s. Almost all constructors of cipher classes have a parameter - the name of the algorithm(s) to be used, and have some sensible default argument for it. The caller can use any string they want, but for the ease of use the root package has defined a number of string constants which contain the names of the most popular algorithms. The constants can be found in the namespace `vm.Aspects.Security.Cryptography.Algorithms`. These algorithm name constants are grouped in several static classes by functional type, e.g. `Asymmetric`, `Signature`, `Hash`, etc. Each one of them has a constant member `Default` which is equal to the most frequently (IMO) used algorithm. For example the constant `vm.Aspects.Security.Cryptography.Algorithms.Symmetric.Default` is equal to the constant `vm.Aspects.Security.Cryptography.Algorithms.Symmetric.Aes`, which in turn is assigned the constant `"AESCryptoServiceProvider"`.

### `ISymmetricAlgorithmFactory`

This interface is responsible for providing a concrete implementation of the abstract class `SymmetricAlgorithm`. The algorithm is specified as a string. The interface hides the details of the inferring of the concrete algorithm class in the method `Initialize` and abstracts the details of creating the instance behind the method `Create`.

### `IHashAlgorithmFactory`
Very similar to `ISymmetricAlgorithmFactory`, this interface is responsible for providing a concrete implementation of the class `HashAlgorithm`.

## Key management

The ciphers encrypt the protected text with symmetric algorithms like AES, using session (or symmetric) keys which are encrypted using asymmetric algorithms, like RSA. In order to distribute the symmetric key throughout a web farm or to simply archive it the so encrypted session key must be stored for multiple uses into a file or in the crypto-package itself. It is clear that in the former case there must be some operational procedures for managing the symmetric (or session) key:

* Ability to create a new key, export it and import it
* If the key is stored in external storage (e.g. file, database, cloud blob, HSM secret, etc.), there must be a mechanism to address that storage (e.g. full path and file name or URL),
* Store, retrieve and check for existence in the so addressed storage.

These behaviors are defined by the following interfaces.

### `IKeyManagement`
This interface defines the methods for creation, import and export of the symmetric key. It is implemented by the ciphers themselves. It may feel as this responsibilities do not belong to the cipher - these methods are needed mostly for operational purposes, but they bind or extract the created, imported or exported key to or from the cryptographic algorithm. It is implemented by the ciphers which store the encrypted symmetric key in an external media and leverage `IKeyStorage` and `IKeyLocationStrategy` (see below).

>*Note that the interface `IKeyManagement` and the utility `KeyFile` import and export the keys in clear text. When handling the clear text of the keys, please use the policies and best practices similar to the ones for handling secret, security artifacts like certificates containing private keys.*

### `IKeyLocationStrategy`

The interface defines a single method `GetKeyLocation` which translates some application-specific, **_logical name_** of the key to an identifier relevant to the physical location where the key is stored - **_physical name_**. E.g. the identifier `"EncryptionKey"` can be translated to the file name `"C:\users\vm\ApplicationData\MyApplication\Encryption.key"` or a key vault secret URL. Think of the logical name as a level of indirection to the physical name of the key, so you do not need to change the name of the key in different environments - just inject different location strategies that translate one and the same logical name to different physical names.

### `IKeyStorage`
This interface deals with the problems of storing and retrieving the key from the external media (e.g. file, blob, HSM secret). The interface defines methods for checking the existence of the key, store and retrieve the encrypted text of the key in the media, and deleting the media containing the key. The store and retrieve operations (`GetKey` and `PutKey`) have also asynchronous versions. The caller can provide their own implementation of this interface as a parameter to the cipher constructor (possibly injected with DI container.) If the caller does not provide an object implementing the interface the library will use the internal class `KeyFileStorage`, which stores and loads the key to a file on the local file system.

## `DefaultServices`

References to all of the supporting interfaces above can be passed-in (injected) in the ciphers' constructors but if you leave these parameters to the default `null` arguments, the constructors will use internal implementation classes from the namespace vm.Aspects.Security.Cryptography.Ciphers.DefaultServices.

### `SymmetricAlgorithmFactory` and `HashAlgorithmFactory`

These are the internal, default implementations of `ISymmetricAlgorithmFactory` and `IHashAlgorithmFactory`. They simply create the corresponding objects from .NET. Therefore they rarely need to be replaced unless new algorithms that are not present in `System.Security.Cryptography` must be used.

The internal implementation of `ISymmetricAlgorithmFactory` follows this strategy for resolving and instantiating the algorithm:

1. If the caller specified an algorithm name (in the cipher's constructor parameter) it is passed and memorized by the method `ISymmetricAlgorithmFactory.Initialize`, after that the factory in the method `ISymmetricAlgorithmFactory.Create` will produce an algorithm by invoking `SymmetricAlgorithm.Create` with parameter the specified algorithm name. 
2. Otherwise, the factory will assume the default symmetric algorithm AES and encryption key with a length of 256 bit.

`HashAlgorithmFactory` works very much like `SymmetricAlgorithmFactory` with the help of `HashAlgorithm.Create`. The default hash algorithm is SHA256.

### `KeyFileLocationStrategy`, `KeyFileStore` and `IKeyManagement`

`KeyFileLocationStrategy` is the default internal implementation of `IKeyLocationStrategy`. It implements the following strategy for determining the physical name of the key file as simple as the following:
1.	If the caller passes a non-empty string, the method will assume that this is the physical path and filename and will return it as is; otherwise,
2.	Simply returns the file name `"Key.key"`

The `KeyFileStore` is the default implementation of `IKeyStore` it stores and retrieves the bytes of the encrypted symmetric key from the local file system.

### Utilities
Based on the previous three interfaces the Ciphers solution in GitHub includes [two CLI utilities](https://github.com/vmelamed/vm/tree/master/Aspects/Security/Cryptography/Ciphers/tools) which use the default implementations of the interfaces:
* **`KeyFile`** gives access to the key management functions of the various ciphers through easy to use command line interface
* **`FileCrypt`** encrypts and decrypts files using the `EncryptedNewKeyCipher` cipher (see below.)

## Main Interfaces and Implementation Classes

There are two main groups of interfaces which define the behavior most of the classes in the package – one that performs cryptographic tasks on plain data (streams and byte arrays), and the other encrypts and signs XML documents and elements. 
The former is mostly in the package (and namespace) `vm.Aspects.Security.Cryptography.Ciphers` and contains the the definitions and implementations of the interfaces `ICipher` and `IHasher` (which includes also keyed and encrypted hashes - signatures) and their asynchronous descendants `ICipherTasks` and `IHasherTasks`.

The latter group is in `vm.Aspects.Security.Cryptography.Ciphers.Xml` and contains interfaces and implementations of `IXmlCipher` and `IXmlSigner`. The methods in `ICipher` and `IXmlCipher` are used to protect data for confidentiality, i.e. encrypts and decrypts data. `IHasher` and `IXmlSigner` protect data for integrity and possibly authentication, i.e. produces and verifies cryptographically strong hash or signature (encrypted hash) of the data.

The interface `ICypher` and its descendant `ICypherTasks` have 2 pairs of methods and one property: 
* `Encrypt` and `Decrypt` operating on `System.Stream` derived data containers;
* `Encrypt` and `Decrypt` operating on data contained in byte arrays - `byte[]`.
* `Base64Encoded` property controls whether the cypher-texts should be Base64-encoded.

Here are a few basic samples for using the interfaces:

```csharp
using System.IO;
using vm.Aspects.Security.Cryptography.Ciphers;

void EncryptStream(Stream dataStream, Stream encryptedStream)
{
    using (ICipher cipher = GetCipher())
        cipher.Encrypt(dataStream, encryptedStream);
}
 
async void DecryptStreamAsync(Stream encryptedStream, Stream dataStream)
{
    using (var cipher = GetCipher())
        await cipher.Decrypt(encryptedStream, dataStream);
}
 
byte[] HashArray(byte[] data)
{
    using (IHasher hasher = GetHasher())
        return hasher.Hash(data);
}

public void EncryptXmlDocument()
{
    var document = LoadXml(xmlOrder);
 
    GetCipher().Encrypt(document, "/order/billing");
}
```

The class `ICipherExtensions` adds a number of convenient extensions for encryption and decryption of various .NET basic types like `String`, the integer types, `Decimal`, `Double`, `DateTime`, `Guid`, etc. It also includes methods for encrypting and hashing of arrays and nullables of the basic types like `int[]`, `long[]`, double?, decimal?, etc. Similarly IHasherExtensions adds a number of extension methods which compute and verify crypto-hashes on the same basic types.

# Ciphers, Hashers and Signers

## ICipher

The root package `vm.Aspects.Security.Cryptography.Ciphers` contains several ciphers implementing `ICipher`. Each of the **`ICipher.Encrypt`** methods in these implementations produce a **_crypto-package_**. A crypto-package contains the encrypted text (the crypto-text,) as well as zero or more non-secret, encryption artifacts like: the length and the contents of the initialization vector; the length and the bytes of the encrypted symmetric key; the length and the data of the hash or the signature, etc.

The property **`ICipher.Base64Encoded`** allows to encode the final package with `Base64` transformation.

The **`ICipher.Decrypt`** methods expect the input data to contain a crypto-package produced by the same type of cipher. E.g. the method `EncryptedKeyCipher.Encrypt` produces a crypto-package that should be decrypted by the method `EncryptedKeyCipher.Decrypt` or `EncryptedKeyCipher.DecryptAsync`. The format of each crypto-package is documented in the respective class's XML comment-document.

### `NullCipher`

This is a trivial implementation of `ICipher`: it does not encrypt or decrypt the input data – it simply copies its clear text to the output destination. This can be useful during the development process to verify what exactly is being encrypted and decrypted: e.g. to make sure that the PII data is being inserted in and read from the right columns of the database. It can be used also in unit-test scenarios as a mock object. The class has a single default constructor:

```csharp
ICipher GetCipher() => new NullCipher();
```

### DpapiCipher
This cipher leverages DPAPI for encrypting and decrypting the data. Therefore it is suitable for programs that are supposed to run only on a single machine: data encrypted on one machine cannot be decrypted on a different machine. This is the fastest cipher but with limited application.

As the name suggests this cipher is actually in the `vm.Aspects.Security.Cryptography.Ciphers.ProtectedData` package.

### `EncryptedKeyCipher`

This class, and the classes derived from it, use some implementation of a symmetric encryption algorithm (by default AES) to protect the confidentiality of the data. The symmetric key is encrypted and stored to a named storage location (e.g. a key file). The symmetric encryption key is protected by encrypting and decrypting it with asymmetric keys stored in a certificate specified in the constructor of the cipher. This significantly simplifies the secure management of the key. In order to obtain the symmetric key from the key file, an attacker has to have access to the certificate containing the private key. Therefore, provided proper certificate management, the key file is well protected and can be freely distributed: copied, e-mailed, etc. However,  the initialization of `EncryptedKeyCipher` is a bit slower than the following `ProtectedKeyCipher` (see below) because of the overhead for asymmetric encryption and certificate retrieval.

`EncryptedKeyCipher` is the base class of all remaining non-XML ciphers. It defines the overall process of encryption, decryption and building the crypto-package in several virtual methods. You might recognize the GoF design pattern of "method template" which invokes a set of virtual methods whose implementation depends on the concrete class.

The following code loads a certificate from the current user's personal certificates store issued to the subject `"vm.EncryptionCipherUnitTest"` and passes it to the constructor to be used for encrypting and decrypting the symmetric key:

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using vm.Aspects.Security.Cryptography;

ICipher GetCipher()
{
    using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
    {
        store.Open(OpenFlags.ReadOnly);
        
        var cert = store
                    .Certificates
                    .Find(X509FindType.FindByTimeValid,DateTime.Now, true)
                    .Find(X509FindType.FindBySubjectName, "vm.EncryptionCipherUnitTest", true)
                    .OfType<X509Certificate2>()
                    .FirstOrDefault()
                        ??  throw new ArgumentException();

        return new EncryptedKeyCipher(cert);
    }
}
```
 
Use the `KeyFile` tool to manage the key file of this cipher.

### `ProtectedKeyCipher`

This cipher is very similar to `EncryptedKeyCipher`. The difference is that the ProtectedKeyCipher class encrypts the symmetric key using DPAPI. (Again, for key management one can use the KeyFile utility.)

The `ProtectedKeyCipher` has a single constructor where all parameters have default values. Here is how to instantiate an object from this class which uses the triple DES encryption instead of the default AES:
```csharp
ICipher GetCipher()
    => new ProtectedKeyCipher(keyFileName, null, null, Algorithms.Symmetric.TripleDes);
```

This cipher is in the `vm.Aspects.Security.Cryptography.ProtectedData` package.
 because it depends on DPAPI.

### `PasswordProtectedKeyCipher`
This cipher derives the symmetric key from a passed-in password. The derivation is intentionally slow process and this cipher should not be used for frequent encryption and decryption with different passwords. Since the key is derived from the password each time, the cipher does not need to manage the symmetric key and it disables the inherited methods of the `IKeyManagement` by doing nothing and returning `null-s. The password is passed to the cipher in the constructor as string. It also takes two additional parameters needed for the password key derivation: number of iterations and salt length. The minimum number of iterations is 4096. Any number below this will result in throwing an exception. The default value is 16384. The more iterations are specified the safer and the slower the key-derivation process is. The minimum salt length is 8 bytes, the default length is 24, and the recommended length is as long as the length of the generated key is, e.g. if the key is 256 bits long the salt should be 32 bytes long.

### `EncryptedNewKeyCipher`
Common problem of the ciphers `EncryptedKeyCipher` and `ProtectedKeyCipher` is that, if the key file is lost or somehow the key is compromised, all protected documents will be inaccessible or will lose their confidentiality. The problem is addressed by the `EncryptedNewKeyCipher` naturally for the price of a certain performance degradation.

This cipher generates a new symmetric encryption key for each document, encrypts the key with a public key from a certificate and stores it in the crypto-package itself. There is no key file, and no key management. If a key is compromised only the document from the same crypto-package is compromised. The drawback is that the entire encryption process is slower.

In the sample above if you simply replace `EncryptedKeyCipher` with `EncryptedNewKeyCipher` you will get a valid working example of instantiating the latter. Since this cipher stores the key in the document the inherited methods of the interface `IKeyManagement` do nothing and return null-s.

### `EncryptedNewKeyHashedCipher`
This cipher adds in the crypto-package the cryptographic hash of the encrypted document. This ensures that the included document has not been modified, guaranteeing the integrity of the document. The hash algorithm is specified in the constructor along with the length of the hash "salt" eliminating the possibility of hash dictionary attacks.

The constructor of the cipher has a parameter which specifies the hash algorithm. By default the hash algorithm uses SHA256. In the following example we use MD5 for hashing and 3DES for symmetric encryption instead:

```csharp
ICipher GetCipher()
    => new EncryptedNewKeyHashedCipher(cert, Algorithms.Hash.MD5, Symmetric.TripleDes);
```

The problem with this cipher is that it does not authenticate the source of the document and therefore is somewhat prone to a man-in-the-middle attack: if the encryption certificate is compromised, so is the hash.

Note that the property `Base64Encoded` is not supported here and setting it to true will throw an `InvalidOperationException`.

### EncryptedNewKeySignedCipher
This cipher replaces the hash from the above class with cryptographic signature which ensures not only the integrity of the document but also the identity of the source. The cipher requires a second (signing) certificate as a parameter in its constructor.

Below is a sample instantiation where the signature is created from SHA256 hash (the default is SHA1) and the symmetric encryption algorithm is Rijndael (the default is AES):

```csharp
ICipher GetCipher()
{
    using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
    {
        store.Open(OpenFlags.ReadOnly);

        var exchangeCert = store.Certificates
                         .Find(X509FindType.FindByTimeValid, DateTime.Now, true)
                         .Find(X509FindType.FindBySubjectName, "vm.Sha256EncryptingCipherUnitTest", true)
                         .OfType<X509Certificate2>()
                         .FirstOrDefault()
                            ?? throw new InvalidOperationException();
 
        var exchangeCert = store.Certificates
                         .Find(X509FindType.FindByTimeValid, DateTime.Now, true)
                         .Find(X509FindType.FindBySubjectName, "vm.Sha256SigningCipherUnitTest", true)
                         .OfType<X509Certificate2>()
                         .FirstOrDefault()
                            ?? throw new InvalidOperationException();
 
        return new EncryptedNewKeySignedCipher(
                    exchangeCert, signCert, Algorithms.Hash.Sha256, Algorithms.Symmetric.Rijndael);
    }
}
```

Note:
* for signing, the cypher supports only the RSA algorithm.
* the property `Base64Encoded` is not supported here and setting it to true will throw `InvalidOperationException`.

## `IHasher`

The Ciphers package contains four hashers: `Hasher`, `KeyedHasher`,  `PasswordHasher`, and `RsaSigner`. Each of these produces a digest of the input text which later can be used to verify that the original text was not tampered with. For strengthening the hash, the interface has a property SaltLength (not used by `KeyedHasher` and `RsaSigner`.) This value by default is 8 but can be overridden in the objects' constructors.

### `Hasher`

The `Hasher` computes and verifies digest of the input text. By default, it uses the SHA256 algorithm but this can be changed to any of the constants from `vm.Aspects.Security.Cryptography.Algorithms.Hash`.

Here is how to instantiate the hasher which will produce MD5 hasher with 16 bytes salt:

```csharp
public override IHasher GetHasher()
    => new Hasher(Algorithms.Hash.MD5, 16);
```

### `KeyedHasher`
The keyed hasher computes a digest of the input text and encrypts it with a symmetric key encrypting algorithm, i.e. computes and verifies _Hashed Message Authentication Code - **HMAC**_. By default, it uses HMACSHA256 algorithm: SHA256 hashing and AES encryption but this can be changed to any of the constants from `vm.Aspects.Security.Cryptography.Algorithms.KeyedHash`.
 
The secret symmetric key is encrypted with asymmetric algorithm (certificate). Retrieving of the key is similar to `EncryptedKeyCipher`: pass the certificate, the logical name, the `IKeyLocationStrategy` and the `IKeyStorage` implementations to retrieve the encrypted key, and using the certificate the hasher will decrypt it. You can use the tool KeyFile to export/import/create HMAC keys.

Here is how to instantiate the hasher which will produce HMACSHA384 hasher:

```csharp
public override IHasher GetHasher()
    => new Hasher(certificate, Algorithms.KeyedHash.HmacSha384, keyFileName);
```

### `RsaSigner`
The signer is a hasher which encrypts the produced hash with the private key of the document's source and the verifier decrypts the signature with the public key of the source, thus verifying the identity of the source. Then the verifying code computes the hash and compares it to the decrypted signature which guarantees the integrity of the document.

Currently the only supported asymmetric algorithm for creating signatures is RSA. When creating the signer the caller has the option to select the hash algorithm, which by default is SHA1.

This sample creates a signer which uses SHA256 for underlying hashing:

```csharp
IHasher GetHasher()
{
    using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
    {
        store.Open(OpenFlags.ReadOnly);

        var certs = store.Certificates
                         .Find(X509FindType.FindByTimeValid, DateTime.Now, true)
                         .Find(X509FindType.FindBySubjectName, "vm.Sha256SignatureCipherUnitTest", true)
                         .OfType<X509Certificate2>()
                         .FirstOrDefault()
                            ?? throw new ArgumentException(); 
        return new RsaSigner(cert, Algorithms.Hash.Sha256);
    }
}
```

### `PasswordHasher`
This hasher can be used to hash a password, e.g. for storing it in identity store in accordance with RFC 2898 (PBKF2 based on HMAC/SHA1). In other words this algorithm is used for generating encryption artifacts from a password (a.k.a. password encryption). It is also one of the few officially acceptable methods for hashing of passwords. This is an iterative method, where the generated hash is dependent on the number of iterations, which makes the method also slow but appropriate for user-interactive, password verification. The more iterations are specified the slower is the process, however the safer is the hash.

The constructor initializes the underlying object with 3 parameters:
1.	Number of iterations (min. 1024, default 16384).
2.	Hash length (min. 24 bytes, default 64 bytes).
3.	Salt length (min. 8 bytes, default 64). It is recommended that the length of the salt is equal to the length of the produced hash.

If any of the above parameters is below its minimum value the constructor will throw an exception.


## `ILightCipher` and `ILightHasher` 

This is not a real cipher but is an interface that can be implemented by a cipher. Some of the ciphers (e.g. `EncryptedNewKey`) use certificates to retrieve the clear text of the symmetric key. Once retrieved, the loaded and the respective public/private keys are not used anymore. If the cipher is used more than once, it would make sense to release the certificate and the related asymmetric algorithms and keys for resource utilization and security reasons. The methods of ILightCipher allow for that - "lighten the cipher" and also can clone the current cipher to a "lightweight cipher". The interface is implemented by the `EncryptedKeyCipher` descendants.

The goal and definition of `ILightHasher` is similar but works with HMAC hashers like `KeyedHasher`.

# XML Ciphers and Signers

The classes from this group can be used to encrypt and/or sign XML documents or selected elements of those according to several standards specified [here](http://www.w3.org/TR/xmlenc-core/) and [here](http://www.w3.org/TR/xmldsig-core/).

## `IXmlCipher`

The interface has only two methods – `Encrypt` and `Decrypt` and a property - `ContentOnly`. 

The property `ContentOnly` specifies whether only the content of the XML elements to be encrypted or the whole elements, including the attributes and the element names.

The method `Encrypt` has three parameters. The first is the XML document that must be encrypted. The second is an XPath expression which selects the elements that need to be replaced by their encrypted representation. If not specified (default argument `null`) the whole document will be encrypted. The third parameter is an optional `XmlNamespaceManager` object which maps the namespace prefixes used in the XPath expression (if any) to XML namespaces.

The method `Decrypt` has only one parameter – the encrypted document.

The XML ciphers support the following symmetric algorithms: DES, 3DES, AES – 128/192/256; the RSA asymmetric algorithm and RSA/SHA – 1/256/384/512 signing.

Here is a sample of an order document:
```xml
<?xml version="1.0" encoding="utf-16"?>
<order>
  <items>
    <item quantity="1">.NET Framework Security</item>
    <item quantity="1">Essential XML Quick Reference</item>
  </items>
  <shipping>
    <to>Joe Smith</to>
    <street>110 Denny Way</street>
    <city>Seattle</city>
    <zip>98109</zip>
  </shipping>
  <billing>
    <paymentInfo type="Visa">
      <number>0000-0000-0000-0000</number>
      <expirationDate>09/15/80</expirationDate>
      <billingAddress>
        <who>Microsoft Corporation</who>
        <street>1 Microsoft Way</street>
        <city>Redmond</city>
        <zip>98052</zip>
      </billingAddress>
    </paymentInfo>
  </billing>
  <billing>
    <paymentInfo type="Visa">
      <number>1111-1111-1111-1111</number>
      <expirationDate>09/15/80</expirationDate>
      <billingAddress>
        <who>Microsoft Corporation</who>
        <street>1 Microsoft Way</street>
        <city>Redmond</city>
        <zip>98052</zip>
      </billingAddress>
    </paymentInfo>
  </billing>
</order>
```

And this is the same document with encrypted billing elements:

```xml
<?xml version="1.0" encoding="utf-16"?>
<order>
  <items>
    <item quantity="1">.NET Framework Security</item>
    <item quantity="1">Essential XML Quick Reference</item>
  </items>
  <shipping>
    <to>Joe Smith</to>
    <street>110 Denny Way</street>
    <city>Seattle</city>
    <zip>98109</zip>
  </shipping>
  <EncryptedData Type="http://www.w3.org/2001/04/xmlenc#Element" xmlns="http://www.w3.org/2001/04/xmlenc#">
    <EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#aes256-cbc" />
    <CipherData>
      <CipherValue>7Dx5aMy/Kyz...4YI5Y</CipherValue>
    </CipherData>
  </EncryptedData>
  <EncryptedData Type="http://www.w3.org/2001/04/xmlenc#Element" xmlns="http://www.w3.org/2001/04/xmlenc#">
    <EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#aes256-cbc" />
    <CipherData>
      <CipherValue>7Dx5aMy/Kyz...SUGR+</CipherValue>
    </CipherData>
  </EncryptedData>
</order>
```

### `ProtectedKeyXmlCipher`
The class implements `IXmlCipher` and `IKeyManagement`. It is very similar to the `ProtectedKeyCipher`: uses symmetric encryption, where the symmetric key is protected with DPAPI, the constructor takes the same parameters and defaults. The class uses `IKeyStorage` and `IKeyLocationStrategy` for locating and storing the symmetric key.

### `EncryptedKeyXmlCipher`

The class is similar to `EncryptedKeyCipher` where the session key is protected by encrypting it with asymmetric key from a certificate.

### `EncryptedNewKeyXmlCipher`

As expected this is similar to `EncryptedKeyXmlCipher`: each document is encrypted with its own symmetric key encrypted by a certificate and stored in the crypto-package of the encrypted elements.

### `EncryptedNewKeySignedXmlCipher`

The cipher adds an enveloped SHA/RSA signature to the document.

## `IXmlSigner`

The XML signing standard specifies three types of signatures which are defined in the `enum SignatureLocation`:
* Detached – the signature is an XML document delivered in a separate document from the signed document.
* Enveloped – the signature is an element enveloped in the XML document and
* Enveloping – the signature is an element which envelops the signed element.

The interface has a property `SignatureLocation` which specifies the type of the signature. Since the only supported XML algorithms by the standards is SHA/RSA, there is only one signer class - `RsaXmlSigner` (see below).

The `bool` property `IncludeKeyInfo` specifies whether the signature should include the optional information about the signing key. Some consider including this information prone to man-in-the-middle attacks.

The method `Sign` has four parameters:
* the XML document;
* the XPath of the signed element(s) or null to sign the whole document;
* a namespace manager if the XPath expression contains any prefixes;
* the last parameter is used only for detached signatures in case the whole document is signed. It must specify the URL of the signed document so that it can be specified in the URI attribute of the Reference element of the signature document.

### `RsaXmlSigner`

This is the only class which implements the IXmlSigner interface.
