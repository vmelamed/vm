if "%VSINSTALLDIR%" NEQ "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\" call "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"
set vmDumperVersion=2.0.0

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
Nuget restore -verbosity quiet

rem ------- build for .NET 4.6.2 -------
set FrameworkVersion=4.6.2
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:OutDir=bin\pack%FrameworkVersion% /m

del /q bin\pack%FrameworkVersion%\*.*
if not exist obj md obj
copy /y project.assets.json obj
msbuild vm.Aspects.Diagnostics.ObjectDumper.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- build for .NET 4.7.1 -------
set FrameworkVersion=4.7.1
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:OutDir=bin\pack%FrameworkVersion% /m

del /q bin\pack%FrameworkVersion%\*.*
if not exist obj md obj
copy /y project.assets.json obj
msbuild vm.Aspects.Diagnostics.ObjectDumper.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- build for .NET 4.7.2 -------
set FrameworkVersion=4.7.2
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:OutDir=bin\pack%FrameworkVersion% /m

del /q bin\pack%FrameworkVersion%\*.*
if not exist obj md obj
copy /y project.assets.json obj
msbuild vm.Aspects.Diagnostics.ObjectDumper.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- Package -------

if /i .%suffix%. EQU .. (
NuGet Pack NuGet\ObjectDumper.nuspec -version %vmDumperVersion% -Prop Configuration=%Configuration%
) else (
NuGet Pack NuGet\ObjectDumper.nuspec -version %vmDumperVersion% -suffix %suffix% -Prop Configuration=%Configuration%
)
if /i .%suffix%. EQU .. (
NuGet Pack NuGet\ObjectDumper.symbols.nuspec -version %vmDumperVersion% -Prop Configuration=%Configuration% -symbols
) else (
NuGet Pack NuGet\ObjectDumper.symbols.nuspec -version %vmDumperVersion% -suffix %suffix% -Prop Configuration=%Configuration% -symbols
)

if /i .%suffix%. NEQ .. ren AspectObjectDumper.%vmDumperVersion%.symbols.nupkg AspectObjectDumper.%vmDumperVersion%-%suffix%.symbols.nupkg

if errorlevel 1 goto exit

if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet

rem ------- Upload to NuGet.org -------

@echo Press any key to push to NuGet.org... > con:
@pause > nul:

if /i .%suffix%. EQU .. (
NuGet Push AspectObjectDumper.%vmDumperVersion%.nupkg -source https://www.nuget.org
) else (
NuGet Push AspectObjectDumper.%vmDumperVersion%-%suffix%.nupkg -source https://www.nuget.org
)

:exit
cd nuget
pause 