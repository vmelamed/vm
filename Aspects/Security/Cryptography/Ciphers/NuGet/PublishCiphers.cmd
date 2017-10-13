if "%VSINSTALLDIR%" EQU "" call "%VS140COMNTOOLS%vsvars32.bat"
set vmCiphersVersion=1.12.4

cd %~dp0..
del *.nupkg
if /i .%1. EQU .. (
	set Configuration=Release
	set suffix=
) else (
	set Configuration=Debug
	set suffix=%1
)

set configuration
set suffix

NuGet Update -self

rem ------- .NET 4.6.2 -------
set FrameworkVersion=4.6.2
set FrameworkVersionConst=DOTNET462
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst% /p:OutDir=bin\pack /m

del /q bin\pack\*.*
msbuild vm.Aspects.Security.Cryptography.Ciphers.csproj %commonBuildOptions%
if errorlevel 1 goto exit

del /q EncryptedKey\bin\pack\*.*
msbuild EncryptedKey\EncryptedKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit

del /q ProtectedKey\bin\pack\*.*
msbuild ProtectedKey\ProtectedKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit

del /q MacKey\bin\pack\*.*
msbuild MacKey\MacKey.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- Package -------
if /i .%suffix%. EQU .. (
NuGet Pack NuGet\Ciphers.nuspec -version %vmCiphersVersion% -Prop Configuration=%Configuration%
) else (
NuGet Pack NuGet\Ciphers.nuspec -version %vmCiphersVersion% -suffix %suffix% -Prop Configuration=%Configuration%
)
if /i .%suffix%. EQU .. (
NuGet Pack NuGet\Ciphers.symbols.nuspec -version %vmCiphersVersion% -Prop Configuration=%Configuration% -symbols
) else (
NuGet Pack NuGet\Ciphers.symbols.nuspec -version %vmCiphersVersion% -suffix %suffix% -Prop Configuration=%Configuration% -symbols
)

if /i .%suffix%. NEQ .. ren Ciphers.%vmCiphersVersion%.symbols.nupkg Ciphers.%vmCiphersVersion%-%suffix%.symbols.nupkg

if errorlevel 1 goto exit

if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet

rem ------- Upload to NuGet.org -------

@echo Press any key to push to NuGet.org... > con:
@pause > nul:

if /i .%suffix%. EQU .. (
NuGet Push Ciphers.%vmCiphersVersion%.nupkg -source https://www.nuget.org
) else (
NuGet Push Ciphers.%vmCiphersVersion%-%suffix%.nupkg -source https://www.nuget.org
)

:exit
cd nuget
pause
 