if "%VSINSTALLDIR%" NEQ "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\" call "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"
set vmExpressionSerializationVersion=1.0.114

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

rem ------- .NET 4.6.2 -------
set FrameworkVersion=4.6.2
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:OutDir=bin\pack%FrameworkVersion% /m

del /q bin\pack\*.*
msbuild vm.Aspects.Linq.Expressions.Serialization.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- .NET 4.6.2 -------
set FrameworkVersion=4.7.1
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:OutDir=bin\pack%FrameworkVersion% /m

del /q bin\pack\*.*
msbuild vm.Aspects.Linq.Expressions.Serialization.csproj %commonBuildOptions%
if errorlevel 1 goto exit

rem ------- Package -------

if /i .%suffix%. EQU .. (
NuGet Pack NuGet\ExpressionSerialization.nuspec -version %vmExpressionSerializationVersion% -Prop Configuration=%Configuration%
) else (
NuGet Pack NuGet\ExpressionSerialization.nuspec -version %vmExpressionSerializationVersion% -suffix %suffix% -Prop Configuration=%Configuration%
)
if /i .%suffix%. EQU .. (
NuGet Pack NuGet\ExpressionSerialization.symbols.nuspec -version %vmExpressionSerializationVersion% -Prop Configuration=%Configuration% -symbols
) else (
NuGet Pack NuGet\ExpressionSerialization.symbols.nuspec -version %vmExpressionSerializationVersion% -suffix %suffix% -Prop Configuration=%Configuration% -symbols
)

if /i .%suffix%. NEQ .. ren AspectExpressionSerialization.%vmExpressionSerializationVersion%.symbols.nupkg AspectExpressionSerialization.%vmExpressionSerializationVersion%-%suffix%.symbols.nupkg

if errorlevel 1 goto exit

if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet

rem ------- Upload to NuGet.org -------

@echo Press any key to push to NuGet.org... > con:
@pause > nul:

if /i .%suffix%. EQU .. (
NuGet Push AspectExpressionSerialization.%vmExpressionSerializationVersion%.nupkg -source https://www.nuget.org
) else (
NuGet Push AspectExpressionSerialization.%vmExpressionSerializationVersion%-%suffix%.nupkg -source https://www.nuget.org
)

:exit
cd nuget
pause 