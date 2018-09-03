rem This little script demo-s the NuGet problem of pushing the symbols package to the wrong URL
rem Remember to bump the number of the beta, e.g. beta.22 -> beta.23

set suffix=beta.24

cd %~dp0..\src
del /q bin\Debug\*.nupkg
dotnet clean --configuration Debug
dotnet build --configuration Debug --version-suffix %suffix%
dotnet pack --include-symbols --include-source --configuration Debug --version-suffix %suffix%
dotnet nuget push bin\Debug\ --source https://www.nuget.org