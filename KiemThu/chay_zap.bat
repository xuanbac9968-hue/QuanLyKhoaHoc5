@echo off
chcp 65001 > nul
title OWASP ZAP Security Demo - QuanLyKhoaHoc5

echo.
echo ================================================================
echo   OWASP ZAP 2.17.0 - Security Test (GUI Live Demo)
echo   Target : http://localhost:5125
echo   Report : D:\QuanLyKhoaHoc5\KiemThu\zap_report\
echo ================================================================
echo.

:: ── Kiem tra web dang chay ─────────────────────────────────────────
echo [1/3] Kiem tra web app...
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost:5125' -TimeoutSec 5 -UseBasicParsing; Write-Host '  OK - HTTP' $r.StatusCode } catch { Write-Host '  LOI:' $_.Exception.Message; exit 1 }"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo  [!] Web chua chay! Hay bat truoc:
    echo      - Mo Visual Studio, nhan F5
    echo      - Cho den khi thay: Now listening on http://localhost:5125
    echo.
    pause
    exit /b 1
)

:: ── Tao thu muc bao cao ────────────────────────────────────────────
set REPORT_DIR=D:\QuanLyKhoaHoc5\KiemThu\zap_report
if not exist "%REPORT_DIR%" mkdir "%REPORT_DIR%"

echo [2/3] Chuan bi ZAP...
set JAVA="C:\Program Files\Eclipse Adoptium\jdk-25.0.3.9-hotspot\bin\java.exe"
set JAR="C:\Program Files\ZAP\Zed Attack Proxy\zap-2.17.0.jar"
set REPORT_FILE=%REPORT_DIR%\zap_live_report.html

echo.
echo ================================================================
echo   ZAP GUI se mo ngay bay gio.
echo.
echo   HUONG DAN DEMO CHO GIAO VIEN:
echo.
echo   [Tab "Quick Start" - ben trai]
echo     1. Nhap URL: http://localhost:5125
echo     2. Chon "Automated Scan"
echo     3. Click nut "Attack" (mau xanh)
echo.
echo   [Trong khi quet - co the xem:]
echo     - Tab "Spider"       : cac trang tim duoc
echo     - Tab "Active Scan"  : cac request kiem tra lo hong
echo     - Tab "Alerts"       : lo hong da phat hien
echo     - Tab "Response"     : noi dung trang web
echo.
echo   [Sau khi xong - xuat bao cao:]
echo     Report menu > Generate Report > Traditional HTML
echo     Luu vao: %REPORT_DIR%
echo.
echo   Hoac dung Automation Plan co san:
echo     Tools > Automation Framework > Open Plan
echo     Chon file: D:\QuanLyKhoaHoc5\KiemThu\zap_plan.yaml
echo     Click Play button
echo ================================================================
echo.

echo [3/3] Dang mo ZAP GUI voi target da san sang...
echo.

%JAVA% -Xmx768m ^
    -jar %JAR% ^
    -quickurl "http://localhost:5125" ^
    -quickout "%REPORT_FILE%" ^
    -quickprogress

echo.
echo ================================================================
echo  XONG! Bao cao HTML tai: %REPORT_FILE%
echo ================================================================
echo.
pause
