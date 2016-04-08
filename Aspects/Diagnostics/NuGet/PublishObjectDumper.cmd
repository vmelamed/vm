pushd
cd %~dp0..
del ..\*.nupkg
NuGet Update -self
call "%VS140COMNTOOLS%vsvars32.bat"
if not .%1.==.. NuGet SetApiKey %1
msbuild vm.Aspects.Diagnostics.ObjectDumper.csproj /t:Rebuild /p:Configuration=Release /m
if errorlevel 1 goto exit
NuGet Pack NuGet\ObjectDumper.nuspec -Prop Configuration=Release -symbols
if errorlevel 1 goto exit
@echo Press any key to push to NuGet... > con:
@pause > nul:
NuGet Push AspectObjectDumper.1.5.3.nupkg
:exit
popd
pause