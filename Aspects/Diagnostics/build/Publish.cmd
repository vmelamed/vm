cd %~dp0..\src

if .%1. EQU .. (
    set PackagePath=bin\Release\
    set Configuration=--configuration Release
    set VersionSuffix=
) else (
    set PackagePath=bin\Debug\
    set Configuration=--configuration Debug
    set VersionSuffix=--version-suffix %1
    )

rem ------- Build and Package -------
dotnet clean %Configuration%
dotnet build %Configuration%
dotnet pack --include-symbols --include-source %Configuration% %VersionSuffix%
if errorlevel 1 goto exit

rem ------- Publish locally -------
if not exist c:\NuGet md c:\NuGet
copy /y %PackagePath%*.nupkg c:\NuGet

rem ------- Upload to NuGet.org -------

@echo Press any key to push to NuGet.org or Ctrl-Break to exit... > con:
@pause > nul:

dotnet nuget push %PackagePath% --source https://www.nuget.org
if errorlevel 1 goto exit
if .%VersionSuffix%. EQU ..  AND .%VersionPrefix%. NEQ .. git tag %VersionPrefix%

:exit
cd ..\build
pause