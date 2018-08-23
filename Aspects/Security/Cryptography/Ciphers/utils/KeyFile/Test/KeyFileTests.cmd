@cd ..\bin\debug
@if .%1. NEQ .. goto %1
@del /q *.key

KeyFile help
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)

:create
@echo ---------------------------------------------------
@echo CREATE TESTS:
@echo ---------------------------------------------------
KeyFile help create
@echo ---------------------------------------------------
@echo       CREATE DPAPI TESTS:
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile create dpapi.key
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile create dpapi.key < ..\..\Test\Yes.txt
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile create dpapi.key < ..\..\Test\No.txt
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile create -m dpapi dpapi.key -o
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
@echo       CREATE CERTIFICATE TESTS:
KeyFile create certificate.key -m certificate -t 919c37ec11a8086a4978e055fc5767ff60aed452
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile create certificate.key -m certificate -t 919c37ec11a8086a4978e055fc5767ff60aed452 < ..\..\Test\Yes.txt
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile create certificate.key -m certificate -t 919c37ec11a8086a4978e055fc5767ff60aed452 < ..\..\Test\No.txt
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile create certificate.key -m certificate -t 919c37ec11a8086a4978e055fc5767ff60aed452 -o
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
@echo       CREATE MAC TESTS:
KeyFile create mac.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile create mac.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f < ..\..\Test\Yes.txt
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile create mac.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f < ..\..\Test\No.txt
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile create mac.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f -o
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@if .%1. NEQ .. goto exit

:export
@echo ---------------------------------------------------
@echo EXPORT TESTS
@echo ---------------------------------------------------
KeyFile help export
@echo ---------------------------------------------------
@echo       EXPORT DPAPI TESTS:
KeyFile export dpapi.key
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile export dpapi.key -m dpapi
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile export certificate.key
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile export mac.key
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
@echo       EXPORT CERTIFICATE TESTS:
KeyFile export certificate.key -m certificate -t 919c37ec11a8086a4978e055fc5767ff60aed452
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile export dpapi.key -m certificate -t 919c37ec11a8086a4978e055fc5767ff60aed452
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile export mac.key -m certificate -t 919c37ec11a8086a4978e055fc5767ff60aed452
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
@echo       EXPORT MAC TESTS:
KeyFile export mac.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile export certificate.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile export dpapi.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
@if .%1. NEQ .. goto exit

:import
@echo ---------------------------------------------------
@echo IMPORT TESTS:
@echo ---------------------------------------------------
KeyFile help import
@echo ---------------------------------------------------
@echo       IMPORT DPAPI TESTS:
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
KeyFile import dpapi.key -k 65-85-F5-15-85-25-F5-25-55-45-65-45-05-B5-25-95-55-75-C5-F5-35-C5-55-95-85-65-E5-85-55-15-75-35
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@KeyFile export dpapi.key
@echo ---------------------------------------------------
KeyFile import -m dpapi dpapi.key -k 65-85-F5-15-85-25-F5-25-55-45-65-45-05-B5-25-95-55-75-C5-F5-35-C5-55-95-85-65-E5-85-55-15-75-35-65-85-F5-15-85-25-F5-25-55-45-65-45-05-B5-25-95-55-75-C5-F5-35-C5-55-95-85-65-E5-85-55-15-75-35
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
@echo       IMPORT CERTIFICATE TESTS:
KeyFile import certificate.key -m certificate -t 919c37ec11a8086a4978e055fc5767ff60aed452 -k 65-85-F5-15-85-25-F5-25-55-45-65-45-05-B5-25-95-55-75-C5-F5-35-C5-55-95-85-65-E5-85-55-15-75-35
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@KeyFile export certificate.key -m certificate -t 919c37ec11a8086a4978e055fc5767ff60aed452
@echo ---------------------------------------------------
KeyFile import certificate.key -m certificate -t 919c37ec11a8086a4978e055fc5767ff60aed452 -k 65-85-F5-15-85-25-F5-25-55-45-65-45-05-B5-25-95-55-75-C5-F5-35-C5-55-95-85-65-E5-85-55-15-75-35-65-85-F5-15-85-25-F5-25-55-45-65-45-05-B5-25-95-55-75-C5-F5-35-C5-55-95-85-65-E5-85-55-15-75-35
@if not errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@echo ---------------------------------------------------
@echo       IMPORT MAC TESTS:
KeyFile import mac.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f -k 65-85-F5-15-85-25-F5-25-55-45-65-45-05-B5-25-95-55-75-C5-F5-35-C5-55-95-85-65-E5-85-55-15-75-35-65-85-F5-15-85-25-F5-25-55-45-65-45-05-B5-25-95-55-75-C5-F5-35-C5-55-95-85-65-E5-85-55-15-75-35
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@KeyFile export mac.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f
@echo ---------------------------------------------------
KeyFile import mac.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f -k 65-85-F5-15-85-25-F5-25-55-45-65-45-05-B5-25-95-55-75-C5-F5-35-C5-55-95-85-65-E5-85-55-15-75-35
@if errorlevel 1 (@echo ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR) else (@echo .)
@KeyFile export mac.key -m mac -t ebebf9c4a54a221a59ee6cfa8117d6533fda846f
@if .%1. NEQ .. goto exit

:exit
@cd ..\..\Test