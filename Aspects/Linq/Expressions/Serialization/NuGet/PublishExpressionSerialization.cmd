if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
set vmExpressionSerialization=1.0.97
set Configuration=Release
pushd

cd %~dp0..
del *.nupkg
NuGet Update -self
if not .%1.==.. NuGet SetApiKey %1

rem ------- .NET 4.5.2 -------
set FrameworkVersion=4.5.2
set FrameworkVersionConst=DOTNET452
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst%;OutDir=bin\%Configuration%%FrameworkVersionConst% /m

msbuild vm.Aspects.Linq.Expressions.Serialization.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- .NET 4.6.2 -------
set FrameworkVersion=4.6.2
set FrameworkVersionConst=DOTNET462
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst%;OutDir=bin\%Configuration%%FrameworkVersionConst% /m

msbuild vm.Aspects.Linq.Expressions.Serialization.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- Package -------
NuGet Pack NuGet\ExpressionSerialization.nuspec -Prop Configuration=Release -symbols
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
if .%1.==.. (
NuGet Push AspectExpressionSerialization.%vmExpressionSerialization%.nupkg -source https://www.nuget.org
) else (
NuGet Push AspectExpressionSerialization.%vmExpressionSerialization%.nupkg -source https://www.nuget.org -ApiKey %1
)

:exit
popd
pause 