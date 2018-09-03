call PublishCiphers.cmd %1
if errorlevel 1 goto exit

call PublishCiphersXml.cmd %1
if errorlevel 1 goto exit

call PublishCiphersProtectedData.cmd %1
if errorlevel 1 goto exit

:exit
cd ..\build
pause