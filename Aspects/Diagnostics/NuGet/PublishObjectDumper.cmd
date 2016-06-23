if not .%vmDumperVersion%.==.. goto afterSets
set vmDumperVersion=1.6.5
set FrameworkVersion=4.6.1
set FrameworkVersionConst=DOTNET461
set Configuration=Release
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst% /m
if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
:afterSets
pushd
cd %~dp0..
del *.nupkg
NuGet Update -self
if not .%1.==.. NuGet SetApiKey %1
msbuild vm.Aspects.Diagnostics.ObjectDumper.csproj %commonBuildOptions%
if errorlevel 1 goto exit
NuGet Pack NuGet\ObjectDumper.nuspec -Prop Configuration=Release -symbols
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
if .%1.==.. (NuGet Push AspectObjectDumper.%vmDumperVersion%.nupkg -source https://www.nuget.org) else (NuGet Push AspectObjectDumper.%vmDumperVersion%.nupkg -source https://www.nuget.org -ApiKey %1)
:exit
popd
pause 