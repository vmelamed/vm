if "%VSINSTALLDIR%" NEQ "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\" call "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"
set vmCiphersVersion=2.0.0

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
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:OutDir=bin\pack%FrameworkVersion% /m

call:buildFor net462

rem ------- .NET 4.7.1 -------
set FrameworkVersion=4.7.1
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:OutDir=bin\pack%FrameworkVersion% /m

call:buildFor net471

rem ------- .NET 4.7.2 -------
set FrameworkVersion=4.7.2
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:OutDir=bin\pack%FrameworkVersion% /m

call:buildFor net472

rem ------- Package -------
if /i .%suffix%. EQU .. (
    NuGet Pack NuGet\Ciphers.nuspec -version %vmCiphersVersion% -Prop Configuration=%Configuration%
    NuGet Pack NuGet\Ciphers.symbols.nuspec -version %vmCiphersVersion% -Prop Configuration=%Configuration% -symbols
) else (
    NuGet Pack NuGet\Ciphers.nuspec -version %vmCiphersVersion% -suffix %suffix% -Prop Configuration=%Configuration%
    NuGet Pack NuGet\Ciphers.symbols.nuspec -version %vmCiphersVersion% -suffix %suffix% -Prop Configuration=%Configuration% -symbols
    ren Ciphers.%vmCiphersVersion%.symbols.nupkg Ciphers.%vmCiphersVersion%-%suffix%.symbols.nupkg
)

if errorlevel 1 goto:eof

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

cd nuget & pause & goto:eof


:buildFor
call:buildProject src vm.Aspects.Security.Cryptography.Ciphers.csproj %1
call:buildProject tools\KeyFile KeyFile.csproj %1
call:buildProject tools\FileCrypt FileCrypt.csproj %1
goto:eof

:buildProject
del /q %1\bin\pack\*.*
if not exist %1\obj md %1\obj
copy /y %1\project.assets.json.%3 %1\obj\project.assets.json
msbuild %1\%2 %commonBuildOptions%
if errorlevel 1 goto:eof
goto:eof