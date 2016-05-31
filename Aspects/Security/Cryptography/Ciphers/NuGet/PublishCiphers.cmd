if not .%vmCiphersVersion%.==.. goto afterSets
set vmCiphersVersion=1.11.12
set FrameworkVersion=4.6.1
set FrameworkVersionConst=DOTNET461
set Configuration=Release
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst% /m
if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
:afterSets
pushd
cd %~dp0..
del *.nupkg
NuGet Update -self
if not .%1.==.. NuGet SetApiKey %1
msbuild vm.Aspects.Security.Cryptography.Ciphers.csproj %commonBuildOptions%
if errorlevel 1 goto exit
msbuild EncryptedKey\EncryptedKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit
msbuild ProtectedKey\ProtectedKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit
msbuild MacKey\MacKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit
NuGet Pack NuGet\Ciphers.nuspec -Prop Configuration=Release -symbols
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
NuGet Push Ciphers.%vmCiphersVersion%.nupkg -source https://www.nuget.org
if .%1.==.. (NuGet Push vm.Aspects.%vmCiphersVersion%.nupkg -source https://www.nuget.org) else (NuGet Push vm.Aspects.%vmCiphersVersion%.nupkg -source https://www.nuget.org -ApiKey %1)
:exit
popd
pause
 