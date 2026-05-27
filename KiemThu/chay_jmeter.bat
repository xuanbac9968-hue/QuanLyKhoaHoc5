@echo off
chcp 65001 > nul
title JMeter Performance Test - QuanLyKhoaHoc5

echo.
echo ==============================================
echo   JMETER PERFORMANCE TEST - QuanLyKhoaHoc5
echo   Web: http://localhost:5125
echo   Test Plan: TestPlan_QuanLyKhoaHoc.jmx
echo ==============================================
echo.

set JMETER_HOME=D:\QuanLyKhoaHoc5\KiemThu\jmeter
set JAVA_HOME=C:\Program Files\Eclipse Adoptium\jdk-25.0.3.9-hotspot
set JMX_FILE=D:\QuanLyKhoaHoc5\KiemThu\TestPlan_QuanLyKhoaHoc.jmx

:: Kiem tra JMeter
if not exist "%JMETER_HOME%\bin\jmeter.bat" (
    echo [LOI] Khong tim thay JMeter tai: %JMETER_HOME%
    echo       Chay lai script cai dat.
    pause
    exit /b 1
)

:: Kiem tra file JMX
if not exist "%JMX_FILE%" (
    echo [LOI] Khong tim thay file test plan: %JMX_FILE%
    pause
    exit /b 1
)

echo [INFO] Dang mo JMeter GUI voi test plan...
echo [INFO] Khi JMeter mo xong, nhan nut Play (Ctrl+R) de bat dau test.
echo [INFO] Xem ket qua o: Summary Report / View Results Tree / Response Time Graph
echo.

cd /d "%JMETER_HOME%\bin"
call jmeter.bat -t "%JMX_FILE%"
