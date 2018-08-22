# -------------------------------------------------------------------------------------------------------------------------------
# Delete all previously created certificates if needed:
# -------------------------------------------------------------------------------------------------------------------------------
foreach ($c in Get-ChildItem Cert:\CurrentUser\my |
            Where-Object {$_.Subject -imatch "vm\..*UnitTest"})
    { Remove-Item ("Cert:\CurrentUser\my\"+$c.Thumbprint); }

foreach ($c in Get-ChildItem cert:\CurrentUser\TrustedPublisher |
            Where-Object {$_.Subject -imatch "vm\..*UnitTest"})
    { Remove-Item ("cert:\CurrentUser\TrustedPublisher\"+$c.Thumbprint); }

# -------------------------------------------------------------------------------------------------------------------------------
# To test SHA1 only signatures use the following commands to create SHA1 only signing certificates (SHA256 will fail with these):
# -------------------------------------------------------------------------------------------------------------------------------
#makecert -r -ss my -pe -n "CN=vm.SignatureCipherUnitTest" -m 12 -sky signature
#makecert -r -pe -n "CN=vm.EncryptionCipherUnitTest" -m 12 -sky exchange -ss my
New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\my -KeyExportPolicy Exportable -Subject "CN=vm.SignatureCipherUnitTest"  -KeyUsage DigitalSignature -KeyUsageProperty Sign -HashAlgorithm sha1 -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" -KeyLength 2048  > $null;
New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\my -KeyExportPolicy Exportable -Subject "CN=vm.EncryptionCipherUnitTest" -KeyUsage DataEncipherment -KeyUsageProperty Decrypt -HashAlgorithm sha1 -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" -KeyLength 2048  > $null;

# ----------------------------------------------------------------------------------------------------
# To test SHA1 and SHA256 signatures use the following commands to create SHA256 signing certificates:
# ----------------------------------------------------------------------------------------------------
#makecert -r -pe -n "CN=vm.Sha256SignatureCipherUnitTest" -m 12 -sky signature -ss my -a sha256 -sp "Microsoft Enhanced RSA and AES Cryptographic Provider" -sy 24
#makecert -r -pe -n "CN=vm.Sha256EncryptionCipherUnitTest" -m 12 -sky exchange -ss my -a sha256 -sp "Microsoft Enhanced RSA and AES Cryptographic Provider" -sy 24
New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\my -KeyExportPolicy Exportable -Subject "CN=vm.Sha256SignatureCipherUnitTest"  -KeyUsage DigitalSignature -KeyUsageProperty Sign -HashAlgorithm sha256 -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" -KeyLength 2048  > $null;
New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\my -KeyExportPolicy Exportable -Subject "CN=vm.Sha256EncryptionCipherUnitTest" -KeyUsage DataEncipherment -KeyUsageProperty Decrypt -HashAlgorithm sha256 -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" -KeyLength 2048  > $null;

# export the created certificates into files, without the private keys
Get-ChildItem cert:\CurrentUser\My | 
    where-object { $_.Subject -like 'CN=vm.*'} | 
        ForEach-Object { 
            $path = $_.Subject -replace "cn=", ""; 
            $path = "$($path).cer"; 
            Export-Certificate -Cert $_ -FilePath $path
        };
# import them to "Other People"
Get-ChildItem vm.*.cer | 
    foreach { 
        Import-Certificate $_ -CertStoreLocation cert:\CurrentUser\TrustedPublisher;
    } > $null;
# clean-updir
Remove-Item vm.*.cer;

if ($Host.Name -eq "ConsoleHost")
{
    Write-Host "Press any key to continue...";
    $Host.UI.RawUI.FlushInputBuffer();
    $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp") > $null;
}
