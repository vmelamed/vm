pushd
if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
set Configuration=Release
set vmAspectsVersion=1.0.72-beta

cd %~dp0..
del *.nupkg
NuGet Update -self
if not .%1.==.. NuGet SetApiKey %1

rem ------- .NET 4.5.2 -------
set FrameworkVersion=4.5.2
set FrameworkVersionConst=DOTNET452
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

:rem ------- .NET 4.6.1 -------
:set FrameworkVersion=4.6.1
:set FrameworkVersionConst=DOTNET461
:set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration%;TargetFrameworkVersion=v%FrameworkVersion%;DefineConstants=%FrameworkVersionConst%;OutDir=bin\%Configuration%%FrameworkVersionConst% /m
:
:msbuild vm.Aspects.csproj %commonBuildOptions%
:if errorlevel 1 goto exit
:cd Model
:msbuild vm.Aspects.Model.csproj %commonBuildOptions%
:if errorlevel 1 goto exit
:cd ..\Parsers
:msbuild vm.Aspects.Parsers.csproj %commonBuildOptions%
:if errorlevel 1 goto exit
:cd ..\FtpTransfer
:msbuild vm.Aspects.FtpTransfer.csproj %commonBuildOptions%
:if errorlevel 1 goto exit
:cd ..\Wcf
:msbuild vm.Aspects.Wcf.csproj %commonBuildOptions%
:if errorlevel 1 goto exit
:cd ..

rem ------- Package -------
NuGet Pack NuGet\vm.Aspects.nuspec -symbols -Prop Configuration=%Configuration%
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
if .%1.==.. (
NuGet Push vm.Aspects.%vmAspectsVersion%.nupkg -source https://www.nuget.org
) else (
NuGet Push vm.Aspects.%vmAspectsVersion%.nupkg -source https://www.nuget.org -ApiKey %1
)

:exit
popd
pause