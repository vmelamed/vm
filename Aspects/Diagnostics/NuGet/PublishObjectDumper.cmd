if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
set vmDumperVersion=1.7.0-beta6
pushd

cd %~dp0..
del *.nupkg
NuGet Update -self
if /i .%1.==.debug. (
	set Configuration=Debug
) else (
	if /i .%1.==.release. (
		set Configuration=Release
	) else (
		set Configuration=Release
		if not .%1.==.. set ApiKey=%1
	)
)

if not .%ApiKey%.==.. NuGet SetApiKey %ApiKey%

rem ------- .NET 4.6.2 -------
set FrameworkVersion=4.6.2
set FrameworkVersionConst=DOTNET462
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst%;OutDir=bin\%Configuration%%FrameworkVersionConst% /m

msbuild vm.Aspects.Diagnostics.ObjectDumper.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- Package -------
NuGet Pack NuGet\ObjectDumper.%Configuration%.nuspec -Prop Configuration=%Configuration% -symbols
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
if .%ApiKey%.==.. (
NuGet Push AspectObjectDumper.%vmDumperVersion%.nupkg -source https://www.nuget.org
) else (
NuGet Push AspectObjectDumper.%vmDumperVersion%.nupkg -source https://www.nuget.org -ApiKey %ApiKey%
)

:exit
popd
pause 