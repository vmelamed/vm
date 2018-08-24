@cd ..\bin\debug
@if .%1. NEQ .. goto %1
@del /q *.key

FileCrypt help
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)

:create
FileCrypt help encrypt
FileCrypt help decrypt

FileCrypt encrypt ..\..\Tests\Test.txt test.encrypted.bin -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f < ..\..\Tests\Yes.txt
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@if .%1. NEQ .. goto exit

FileCrypt decrypt test.encrypted.bin Test.txt -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f < ..\..\Tests\Yes.txt
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@if .%1. NEQ .. goto exit

@comp /M ..\..\Tests\Test.txt Test.txt
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)

FileCrypt encrypt -b ..\..\Tests\Test.txt test.encrypted.txt -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f < ..\..\Tests\Yes.txt
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@if .%1. NEQ .. goto exit

FileCrypt decrypt test.encrypted.txt Test.txt -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f < ..\..\Tests\Yes.txt
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@if .%1. NEQ .. goto exit

@comp /M ..\..\Tests\Test.txt Test.txt
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)

:exit
@cd ..\..\Tests