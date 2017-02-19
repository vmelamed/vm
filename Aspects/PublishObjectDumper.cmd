if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
set vmDumperVersion=1.6.15
set Configuration=Release
pushd

cd %~dp0..
del *.nupkg
NuGet Update -self
if not .%1.==.. NuGet SetApiKey %1

rem ------- .NET 4.0 -------
set FrameworkVersion=4.0
set FrameworkVersionConst=DOTNET40
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst%;OutDir=bin\%Configuration%%FrameworkVersionConst% /m

msbuild vm.Aspects.Diagnostics.ObjectDumper.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- .NET 4.6.2 -------
set FrameworkVersion=4.6.2
set FrameworkVersionConst=DOTNET462
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst%;OutDir=bin\%Configuration%%FrameworkVersionConst% /m

msbuild vm.Aspects.Diagnostics.ObjectDumper.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- Package -------
NuGet Pack NuGet\ObjectDumper.nuspec -Prop Configuration=Release -symbols
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
if .%1.==.. (
NuGet Push AspectObjectDumper.%vmDumperVersion%.nupkg -source https://www.nuget.org
) else (
NuGet Push AspectObjectDumper.%vmDumperVersion%.nupkg -source https://www.nuget.org -ApiKey %1
)

:exit
popd
pause 