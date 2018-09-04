if .%1. EQU .. (
    set $PackagePath=bin\Release\
    set $Configuration=--configuration Release
    set $VersionSuffix=
) else (
    set $PackagePath=bin\Debug\
    set $Configuration=--configuration Debug
    set $VersionSuffix=--version-suffix %1
    )

rem ------- Clean -------
dotnet clean %$Configuration%
del /q %$PackagePath%*.nupkg
rd /s /q %$PackagePath%TempPackages

rem ------- Build -------
dotnet build %$Configuration% %$VersionSuffix%
if errorlevel 1 pause & goto exit
rem del /s /q %$PackagePath%*.deps.json

rem ------- Package -------
dotnet pack --include-symbols --include-source %$Configuration% %$VersionSuffix%
if errorlevel 1 pause & goto exit

rem ------- Publish locally -------
if not exist c:\NuGet md c:\NuGet
copy /y %$PackagePath%*.nupkg c:\NuGet

rem ------- Publish to NuGet.org -------
if .%2. EQU .. (
@echo Press any key to push the packages to NuGet.org or Ctrl-Break to exit... > con:
@pause > nul:
)

md %$PackagePath%TempPackages
move %$PackagePath%*.symbols.nupkg %$PackagePath%TempPackages
dotnet nuget push %$PackagePath%*.nupkg --source https://www.nuget.org/
del %$PackagePath%*.nupkg
move %$PackagePath%TempPackages\*.symbols.nupkg %$PackagePath%
dotnet nuget push %$PackagePath%*.nupkg --source https://nuget.smbsrc.net
del /q /s %$PackagePath%*.nupkg
rd /s /q %$PackagePath%TempPackages

:if .%$VersionSuffix%. EQU .. (
:    git tag vm.Aspects.Security.Cryptography.Ciphers.ProtectedData-%VersionPrefix%
:) else (
:    git tag vm.Aspects.Security.Cryptography.Ciphers.ProtectedData-%VersionPrefix%-%$VersionSuffix%
:)

:exit
set $PackagePath
set $Configuration=
set $VersionSuffix=

if not errorlevel 1 if .%2. EQU .. pause
