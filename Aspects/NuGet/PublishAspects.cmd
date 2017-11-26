if "%VSINSTALLDIR%" NEQ "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\" call "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"
set vmAspectsVersion=1.0.109

cd %~dp0..
del *.nupkg
del /q bin\pack
NuGet Update -self
if /i .%1. EQU .. (
	set Configuration=Release
	set suffix=
) else (
	set Configuration=Debug
	set suffix=%1
	)

rem ------- .NET 4.6.2 -------
set FrameworkVersion=4.6.2
set FrameworkVersionConst=DOTNET462
set commonBuildOptions=/t:Rebuild /p:Configuration=%Configuration% /p:TargetFrameworkVersion=v%FrameworkVersion% /p:DefineConstants=%FrameworkVersionConst% /p:OutDir=bin\pack%FrameworkVersionConst% /m

del /q bin\pack%FrameworkVersionConst%
msbuild vm.Aspects.csproj %commonBuildOptions%
if errorlevel 1 goto exit

cd Model
del /q bin\pack%FrameworkVersionConst%
msbuild vm.Aspects.Model.csproj %commonBuildOptions%
if errorlevel 1 goto exit

cd ..\Parsers
del /q bin\pack%FrameworkVersionConst%
msbuild vm.Aspects.Parsers.csproj %commonBuildOptions%
if errorlevel 1 goto exit

cd ..\FtpTransfer
del /q bin\pack%FrameworkVersionConst%
msbuild vm.Aspects.FtpTransfer.csproj %commonBuildOptions%
if errorlevel 1 goto exit

cd ..\Wcf
del /q bin\pack%FrameworkVersionConst%
msbuild vm.Aspects.Wcf.csproj %commonBuildOptions%
if errorlevel 1 goto exit

cd ..

rem ------- Package -------

if /i .%suffix%. EQU .. (
NuGet Pack NuGet\vm.Aspects.nuspec -version %vmAspectsVersion% -Prop Configuration=%Configuration%
) else (
NuGet Pack NuGet\vm.Aspects.nuspec -version %vmAspectsVersion% -suffix %suffix% -Prop Configuration=%Configuration%
)
if /i .%suffix%. EQU .. (
NuGet Pack NuGet\vm.Aspects.symbols.nuspec -version %vmAspectsVersion% -Prop Configuration=%Configuration% -symbols
) else (
NuGet Pack NuGet\vm.Aspects.symbols.nuspec -version %vmAspectsVersion% -suffix %suffix% -Prop Configuration=%Configuration% -symbols
)

if /i .%suffix%. NEQ .. ren vm.Aspects.%vmAspectsVersion%.symbols.nupkg vm.Aspects.%vmAspectsVersion%-%suffix%.symbols.nupkg

if errorlevel 1 goto exit

if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet

rem ------- Upload to NuGet.org -------

@echo Press any key to push to NuGet.org... > con:
@pause > nul:

if /i .%suffix%. EQU .. (
NuGet Push vm.Aspects.%vmAspectsVersion%.nupkg -source https://www.nuget.org
) else (
NuGet Push vm.Aspects.%vmAspectsVersion%-%suffix%.nupkg -source https://www.nuget.org
)

:exit
cd nuget
pause