@echo off
chcp 65001 > nul
title SonarQube Code Quality Analysis - QuanLyKhoaHoc5

echo.
echo ================================================================
echo   SONARQUBE 26.5.0 - Code Quality Analysis
echo   Project : QuanLyKhoaHoc5 (ASP.NET Core MVC)
echo   Server  : http://localhost:9000
echo   Scanner : dotnet-sonarscanner 11.2.1
echo ================================================================
echo.

:: ── Kiem tra SonarQube dang chay ────────────────────────────────
echo [1/4] Kiem tra SonarQube server...
powershell -Command "try { $r = (Invoke-WebRequest -Uri 'http://localhost:9000/api/system/status' -TimeoutSec 5 -UseBasicParsing).Content | ConvertFrom-Json; if ($r.status -eq 'UP') { Write-Host '  OK - SonarQube' $r.version } else { Write-Host '  WARN: Status =' $r.status } } catch { Write-Host '  LOI: SonarQube chua chay!'; Write-Host '  Khoi dong: D:\sonarqube-26.5.0.122743\bin\windows-x86-64\StartSonar.bat'; exit 1 }"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo  [!] SonarQube chua chay. Hay khoi dong truoc:
    echo      start D:\sonarqube-26.5.0.122743\bin\windows-x86-64\StartSonar.bat
    echo.
    pause
    exit /b 1
)

:: ── Token phan tich (admin/Admin@2026) ──────────────────────────
set TOKEN=sqa_d40db0e27f23e9c1f98654eb5a80fdfde94c4c9c
set PROJECT_KEY=QuanLyKhoaHoc5
set SOURCE_DIR=D:\QuanLyKhoaHoc5

:: ── Step 1: Begin ───────────────────────────────────────────────
echo.
echo [2/4] Bat dau phan tich (sonarscanner begin)...
cd /d %SOURCE_DIR%
dotnet sonarscanner begin ^
    /k:"%PROJECT_KEY%" ^
    /d:sonar.host.url="http://localhost:9000" ^
    /d:sonar.token="%TOKEN%" ^
    /d:sonar.exclusions="KiemThu/**,**/wwwroot/**,**/Migrations/**,**/obj/**,**/bin/**" ^
    /d:sonar.cpd.exclusions="**/*.html,**/*.cshtml" ^
    /d:sonar.test.exclusions="KiemThu/**"

if %ERRORLEVEL% NEQ 0 (
    echo [LOI] sonarscanner begin that bai!
    pause
    exit /b 1
)

:: ── Step 2: Build ───────────────────────────────────────────────
echo.
echo [3/4] Build project...
dotnet build "%SOURCE_DIR%\QuanLyKhoaHoc5.Web\QuanLyKhoaHoc5.Web.csproj" --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo [LOI] Build that bai!
    pause
    exit /b 1
)

:: ── Step 3: End ─────────────────────────────────────────────────
echo.
echo [4/4] Ket thuc phan tich va gui len SonarQube (1-3 phut)...
dotnet sonarscanner end /d:sonar.token="%TOKEN%"
if %ERRORLEVEL% NEQ 0 (
    echo [LOI] sonarscanner end that bai!
    pause
    exit /b 1
)

:: ── Mo browser ──────────────────────────────────────────────────
echo.
echo ================================================================
echo  XONG! Mo browser de xem ket qua...
echo  Dashboard: http://localhost:9000/dashboard?id=%PROJECT_KEY%
echo ================================================================
echo.
start "" "http://localhost:9000/dashboard?id=%PROJECT_KEY%"
pause
