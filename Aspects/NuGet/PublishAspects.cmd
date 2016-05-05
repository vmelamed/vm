if not .%vmAspectsVersion%.==.. goto afterSets
set vmAspectsVersion=1.0.54-beta
set FrameworkVersion=4.6.1
set FrameworkVersionConst=DOTNET461
set Configuration=Release
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst% /m
:afterSets
pushd
cd %~dp0..
del *.nupkg
rem NuGet Update -self
if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
if not .%1.==.. NuGet SetApiKey %1
msbuild vm.Aspects.csproj %commonBuildOptions%
cd Model
msbuild vm.Aspects.Model.csproj %commonBuildOptions%
cd ..\Parsers
msbuild vm.Aspects.Parsers.csproj %commonBuildOptions%
cd ..\Wcf
msbuild vm.Aspects.Wcf.csproj %commonBuildOptions%
if errorlevel 1 goto exit
cd ..
NuGet Pack NuGet\vm.Aspects.nuspec -symbols -Prop Configuration=%Configuration%
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
NuGet Push vm.Aspects.%vmAspectsVersion%.nupkg -source https://www.nuget.org 
:exit
popd
pause