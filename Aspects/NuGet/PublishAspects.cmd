pushd
cd %~dp0..
del *.nupkg
NuGet Update -self
if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
if not .%1.==.. NuGet SetApiKey %1
msbuild vm.Aspects.csproj /t:Rebuild /p:Configuration=Release /m
cd Model
msbuild vm.Aspects.Model.csproj /t:Rebuild /p:Configuration=Release /m
cd ..\Parsers
msbuild vm.Aspects.Parsers.csproj /t:Rebuild /p:Configuration=Release /m
cd ..\Wcf
msbuild vm.Aspects.Wcf.csproj /t:Rebuild /p:Configuration=Release /m
if errorlevel 1 goto exit
cd ..
NuGet Pack NuGet\vm.Aspects.nuspec -symbols -Prop Configuration=Release
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
NuGet Push vm.Aspects.1.0.51-beta.nupkg -source https://www.nuget.org 
:exit
popd
pause