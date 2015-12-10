pushd
cd %~dp0..
NuGet Update -self
call "%VS140COMNTOOLS%vsvars32.bat"
if not .%1.==.. NuGet SetApiKey %1
msbuild vm.Aspects.Security.Cryptography.Ciphers.csproj /t:Rebuild /p:Configuration=Release40 /p:TargetFrameworkVersion=v4.0
if errorlevel 1 goto exit
msbuild vm.Aspects.Security.Cryptography.Ciphers.csproj /t:Rebuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.5
if errorlevel 1 goto exit
msbuild EncryptedKey\EncryptedKey.csproj /t:Rebuild /p:Configuration=Release40 /p:TargetFrameworkVersion=v4.0
if errorlevel 1 goto exit
msbuild EncryptedKey\EncryptedKey.csproj /t:Rebuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.5
if errorlevel 1 goto exit
msbuild ProtectedKey\ProtectedKey.csproj /t:Rebuild /p:Configuration=Release40 /p:TargetFrameworkVersion=v4.0
if errorlevel 1 goto exit
msbuild ProtectedKey\ProtectedKey.csproj /t:Rebuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.5
if errorlevel 1 goto exit
NuGet Pack NuGet\Ciphers.nuspec -Prop Configuration=Release
if errorlevel 1 goto exit
@echo Press any key to push to NuGet... > con:
@pause > nul:
NuGet Push *.nupkg
:exit
del *.nupkg
popd
pause
