if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
set vmDumperVersion=1.7.1
pushd

cd %~dp0..
del *.nupkg
NuGet Update -self
if /i .%1.==.. (
	set Configuration=Release
	set suffix=
) else (
	set Configuration=Debug
	set suffix=%1
)

set configuration
set suffix

rem ------- build for .NET 4.6.2 -------
set FrameworkVersion=4.6.2
set FrameworkVersionConst=DOTNET462
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst%;OutDir=bin\%Configuration%%FrameworkVersionConst% /m

msbuild vm.Aspects.Diagnostics.ObjectDumper.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- Package -------

if /i .%suffix%.==.. (
NuGet Pack NuGet\ObjectDumper.nuspec -version %vmDumperVersion% -Prop Configuration=%Configuration% -symbols
) else (
NuGet Pack NuGet\ObjectDumper.nuspec -version %vmDumperVersion% -suffix %suffix% -Prop Configuration=%Configuration% -symbols
ren AspectObjectDumper.%vmDumperVersion%.symbols.nupkg AspectObjectDumper.%vmDumperVersion%-%suffix%.symbols.nupkg
)

if errorlevel 1 goto exit

if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet

rem ------- Upload to NuGet.org -------

@echo Press any key to push to NuGet.org... > con:
@pause > nul:

if /i .%suffix%.==.. (
NuGet Push AspectObjectDumper.%vmDumperVersion%.nupkg -source https://www.nuget.org
) else (
NuGet Push AspectObjectDumper.%vmDumperVersion%-%suffix%.nupkg -source https://www.nuget.org
)

:exit
popd
pause 