# -------------------------------------------------------------------------------------------------------------------------------
# To test SHA1 only signatures use the following commands to create SHA1 only signing certificates (SHA256 will fail with these):
# -------------------------------------------------------------------------------------------------------------------------------
makecert -r -pe -n "CN=vm.SignatureCipherUnitTest" -m 12 -sky signature -ss my
makecert -r -pe -n "CN=vm.EncryptionCipherUnitTest" -m 12 -sky exchange -ss my

# ----------------------------------------------------------------------------------------------------
# To test SHA1 and SHA256 signatures use the following commands to create SHA256 signing certificates:
# ----------------------------------------------------------------------------------------------------
makecert -r -pe -n "CN=vm.Sha256SignatureCipherUnitTest" -m 12 -sky signature -ss my -a sha256 -sp "Microsoft Enhanced RSA and AES Cryptographic Provider" -sy 24
makecert -r -pe -n "CN=vm.Sha256EncryptionCipherUnitTest" -m 12 -sky exchange -ss my -a sha256 -sp "Microsoft Enhanced RSA and AES Cryptographic Provider" -sy 24

# export the created certificates into files, without the private keys
dir cert:\CurrentUser\My | 
	where-object { $_.Subject -like 'CN=vm.*'} | 
		foreach-object { 
			$path = $_.Subject -replace "cn=", ""; 
			$path = "$($path).cer"; 
			export-certificate -cert $_ -filepath $path
		};
# import them to "Other People"
dir vm.*.cer | 
	foreach { 
		import-certificate $_ -certstorelocation cert:\CurrentUser\TrustedPublisher 
	} > $null;
# clean-up
del vm.*.cer;

if ($Host.Name -eq "ConsoleHost")
{
	Write-Host "Press any key to continue...";
	$Host.UI.RawUI.FlushInputBuffer();
	$Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp") > $null;
}
 