if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
set vmCiphersVersion=1.11.19
set Configuration=Release
pushd

cd %~dp0..
del *.nupkg
NuGet Update -self
if not .%1.==.. NuGet SetApiKey %1

rem ------- .NET 4.5.2 -------
set FrameworkVersion=4.5.2
set FrameworkVersionConst=DOTNET452
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration%;TargetFrameworkVersion=v%FrameworkVersion%;DefineConstants=%FrameworkVersionConst%;OutDir=bin\%Configuration%%FrameworkVersionConst% /m

msbuild vm.Aspects.Security.Cryptography.Ciphers.csproj %commonBuildOptions%
if errorlevel 1 goto exit
msbuild EncryptedKey\EncryptedKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit
msbuild ProtectedKey\ProtectedKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit
msbuild MacKey\MacKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- .NET 4.6.2 -------
set FrameworkVersion=4.6.2
set FrameworkVersionConst=DOTNET462
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration%;TargetFrameworkVersion=v%FrameworkVersion%;DefineConstants=%FrameworkVersionConst%;OutDir=bin\%Configuration%%FrameworkVersionConst% /m

msbuild vm.Aspects.Security.Cryptography.Ciphers.csproj %commonBuildOptions%
if errorlevel 1 goto exit
msbuild EncryptedKey\EncryptedKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit
msbuild ProtectedKey\ProtectedKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit
msbuild MacKey\MacKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- Package -------
NuGet Pack NuGet\Ciphers.nuspec -Prop Configuration=%Configuration% -symbols
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
if .%1.==.. (
NuGet Push Ciphers.%vmCiphersVersion%.nupkg -source https://www.nuget.org
) else (
NuGet Push Ciphers.%vmCiphersVersion%.nupkg -source https://www.nuget.org -ApiKey %1
)

:exit
popd
pause
 