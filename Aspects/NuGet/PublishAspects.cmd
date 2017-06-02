pushd
if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
set vmAspectsVersion=1.0.104-beta6

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
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration%;TargetFrameworkVersion=v%FrameworkVersion%;DefineConstants=%FrameworkVersionConst%;OutDir=bin\%Configuration%%FrameworkVersionConst% /m

msbuild vm.Aspects.csproj %commonBuildOptions%
if errorlevel 1 goto exit
cd Model
msbuild vm.Aspects.Model.csproj %commonBuildOptions%
if errorlevel 1 goto exit
cd ..\Parsers
msbuild vm.Aspects.Parsers.csproj %commonBuildOptions%
if errorlevel 1 goto exit
cd ..\FtpTransfer
msbuild vm.Aspects.FtpTransfer.csproj %commonBuildOptions%
if errorlevel 1 goto exit
cd ..\Wcf
msbuild vm.Aspects.Wcf.csproj %commonBuildOptions%
if errorlevel 1 goto exit
cd ..

rem ------- Package -------
NuGet Pack NuGet\vm.Aspects.%Configuration%.nuspec -symbols -Prop Configuration=%Configuration%
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
if .%ApiKey%.==.. (
NuGet Push vm.Aspects.%vmAspectsVersion%.nupkg -source https://www.nuget.org
) else (
NuGet Push vm.Aspects.%vmAspectsVersion%.nupkg -source https://www.nuget.org -ApiKey %ApiKey%
)

:exit
popd
pause