if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
set vmExpressionSerialization=1.0.105-beta
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

msbuild vm.Aspects.Linq.Expressions.Serialization.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- Package -------
NuGet Pack NuGet\ExpressionSerialization.%Configuration%.nuspec -Prop Configuration=Release -symbols
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
if .%ApiKey%.==.. (
NuGet Push AspectExpressionSerialization.%vmExpressionSerialization%.nupkg -source https://www.nuget.org
) else (
NuGet Push AspectExpressionSerialization.%vmExpressionSerialization%.nupkg -source https://www.nuget.org -ApiKey %ApiKey%
)

:exit
popd
pause 