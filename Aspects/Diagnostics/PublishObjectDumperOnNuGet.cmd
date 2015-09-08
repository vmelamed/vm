NuGet Update -self
call "%VS110COMNTOOLS%vsvars32.bat"
if not .%1.==.. NuGet SetApiKey %1
msbuild vm.Aspects.Diagnostics.ObjectDumper.csproj /t:Rebuild /p:Configuration=Release
NuGet Pack ObjectDumper.nuspec -Prop Configuration=Release
NuGet Push *.nupkg
del *.nupkg
pause