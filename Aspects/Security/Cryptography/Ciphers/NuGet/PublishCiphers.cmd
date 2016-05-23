pushd
cd %~dp0..
del *.nupkg
NuGet Update -self
if "%VSINSTALLDIR%"=="" call "%VS140COMNTOOLS%vsvars32.bat"
if not .%1.==.. NuGet SetApiKey %1
msbuild vm.Aspects.Security.Cryptography.Ciphers.csproj /t:Rebuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.6 /p:DefineConstants="DOTNET46" /m
if errorlevel 1 goto exit
msbuild EncryptedKey\EncryptedKey.csproj /t:Rebuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.6 /p:DefineConstants="DOTNET46" /m
if errorlevel 1 goto exit
msbuild ProtectedKey\ProtectedKey.csproj /t:Rebuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.6 /p:DefineConstants="DOTNET46" /m
if errorlevel 1 goto exit
msbuild MacKey\MacKey.csproj /t:Rebuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.6 /p:DefineConstants="DOTNET46" /m
if errorlevel 1 goto exit
NuGet Pack NuGet\Ciphers.nuspec -Prop Configuration=Release -symbols
if errorlevel 1 goto exit
if not exist c:\NuGet md c:\NuGet
copy /y *.nupkg c:\NuGet
@echo Press any key to push to NuGet.org... > con:
@pause > nul:
NuGet Push Ciphers.1.11.12.nupkg -source https://www.nuget.org
:exit
popd
pause
