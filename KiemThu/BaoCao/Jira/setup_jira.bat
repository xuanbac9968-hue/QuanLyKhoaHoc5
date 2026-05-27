@echo off
chcp 65001 > nul
title Jira Integration Setup – QuanLyKhoaHoc5

echo.
echo ================================================================
echo   JIRA CLOUD INTEGRATION – QuanLyKhoaHoc5
echo   Jira URL : https://xuanbac9968.atlassian.net
echo   Project  : NNL (QuanLyKhoaHoc5)
echo   Email    : xuanbac9968@gmail.com
echo ================================================================
echo.

cd /d "%~dp0"

:: ── Kiểm tra Python ───────────────────────────────────────────────
python --version >nul 2>&1
if errorlevel 1 (
    echo [LOI] Python chua duoc cai dat.
    echo       Tai tai: https://python.org/downloads
    pause & exit /b 1
)

:: Cài requests nếu chưa có
python -c "import requests" >nul 2>&1
if errorlevel 1 (
    echo [INFO] Cai dat requests...
    pip install requests -q
)

:: ── Kiểm tra token ────────────────────────────────────────────────
if exist "JIRA_TOKEN.txt" (
    set /p TOKEN=<JIRA_TOKEN.txt
    echo [OK] Da tim thay token: !TOKEN:~0,10!...
    goto :run_script
)

:: ── Hướng dẫn tạo token ──────────────────────────────────────────
echo.
echo ================================================================
echo  BUOC 1: TAO JIRA API TOKEN
echo ================================================================
echo.
echo  Trinh duyet se mo trang Atlassian API Tokens.
echo  Thuc hien:
echo    1. Click "Create API token"
echo    2. Label: "QuanLyKhoaHoc5-Integration"
echo    3. Click "Create"
echo    4. Copy token (chi hien 1 lan!)
echo.
echo  Mo browser trong 3 giay...
timeout /t 3 /nobreak > nul
start "" "https://id.atlassian.com/manage-profile/security/api-tokens"

echo.
echo ================================================================
echo  BUOC 2: DAN TOKEN VAO DAY
echo ================================================================
echo.
set /p TOKEN="Nhap Jira API token: "

if "%TOKEN%"=="" (
    echo [LOI] Token trong! Thoat.
    pause & exit /b 1
)

:: Lưu token
echo %TOKEN%> JIRA_TOKEN.txt
echo [OK] Da luu token vao JIRA_TOKEN.txt

:run_script
echo.
echo ================================================================
echo  BUOC 3: CHAY JIRA INTEGRATION SCRIPT
echo ================================================================
echo.

:: Test connection
echo [1/4] Test ket noi Jira...
python tao_jira_issues.py --token %TOKEN% --list
if errorlevel 1 (
    echo [CANH BAO] Co loi ket noi. Kiem tra lai token.
    del JIRA_TOKEN.txt >nul 2>&1
    pause & exit /b 1
)

echo.
echo [2/4] Tao issues tu SonarQube bugs...
python tao_jira_issues.py --token %TOKEN%
if errorlevel 1 (
    echo [CANH BAO] Mot so issue co the da ton tai.
)

echo.
echo [3/4] Mo Jira Board trong browser...
timeout /t 2 /nobreak > nul
start "" "https://xuanbac9968.atlassian.net/jira/software/projects/NNL/boards"

echo.
echo [4/4] Mo HTML Jira Board (offline)...
start "" "%~dp0Jira_BugBoard.html"

echo.
echo ================================================================
echo  HOAN THANH!
echo  - Jira Board: https://xuanbac9968.atlassian.net/jira/projects/NNL
echo  - Offline Board: Jira_BugBoard.html (da mo trong browser)
echo  - Issues duoc tao: kiem tra tren Jira Cloud
echo ================================================================
echo.
pause
