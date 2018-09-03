cd %~dp0..\Xml\src

if .%1. EQU .. (
    set $PackagePath=bin\Release\
    set $Configuration=--configuration Release
    set $VersionSuffix=
    set $DebugIncludes=
) else (
    set $PackagePath=bin\Debug\
    set $Configuration=--configuration Debug
    set $VersionSuffix=--version-suffix %1
    set $DebugIncludes=--include-source --include-symbols 
    )

rem ------- Clean -------
dotnet clean %$Configuration%
del /q %$PackagePath%*.nupkg

rem ------- Build -------
dotnet build %$Configuration%  %$VersionSuffix%
if errorlevel 1 pause & goto exit

rem ------- Package -------
dotnet pack %$DebugIncludes% %$Configuration% %$VersionSuffix%
if errorlevel 1 pause & goto exit

rem ------- Publish locally -------
if not exist c:\NuGet md c:\NuGet
copy /y %$PackagePath%*.nupkg c:\NuGet

rem ------- Upload to NuGet.org -------
if .%2. EQU .. (
@echo Press any key to push to NuGet.org or Ctrl-Break to exit... > con:
@pause > nul:
)

dotnet nuget push %$PackagePath% --source https://www.nuget.org
if errorlevel 1 pause & goto exit

if .%$VersionSuffix%. EQU .. (
    git tag vm.Aspects.Security.Cryptography.Ciphers.ProtectedData-%VersionPrefix%
) else (
    git tag vm.Aspects.Security.Cryptography.Ciphers.ProtectedData-%VersionPrefix%-%$VersionSuffix%
)

:exit
if not errorlevel 1 if .%2. EQU .. pause
cd ..\..\build
