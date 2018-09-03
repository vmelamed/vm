pushd ..

cd src
call %~dp0\buildProject.cmd %1
if errorlevel 1 goto exit

cd ..\Xml\src
call %~dp0\buildProject.cmd %1
if errorlevel 1 goto exit

cd ..\ProtectedData\src
call %~dp0\buildProject.cmd %1
if errorlevel 1 goto exit

:exit
popd
pause