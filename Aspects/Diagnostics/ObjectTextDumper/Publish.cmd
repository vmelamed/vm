cd %~dp0..\src

if .%1. EQU .. (
    set $PackagePath=bin\Release\
    set $Configuration=--configuration Release
    set $VersionSuffix=
    set $DebugIncludes=--include-symbols
) else (
    set $PackagePath=bin\Debug\
    set $Configuration=--configuration Debug
    set $VersionSuffix=--version-suffix %1
    set $DebugIncludes=--include-symbols --include-source
    )

rem ------- Build and Package -------
dotnet clean %$Configuration%
del %$PackagePath%*.nupkg
dotnet build %$Configuration%
dotnet pack %$DebugIncludes% %$Configuration% %$VersionSuffix%
if errorlevel 1 goto exit

rem ------- Publish locally -------
if not exist c:\NuGet md c:\NuGet
copy /y %$PackagePath%*.nupkg c:\NuGet

rem ------- Upload to NuGet.org -------

@echo Press any key to push to NuGet.org or Ctrl-Break to exit... > con:
@pause > nul:

dotnet nuget push %$PackagePath% --source https://www.nuget.org --symbol-source https://nuget.smbsrc.net
if errorlevel 1 goto exit
if .%$VersionSuffix%. EQU ..  AND .%VersionPrefix%. NEQ .. git tag %VersionPrefix%

:exit
cd ..\build
pause