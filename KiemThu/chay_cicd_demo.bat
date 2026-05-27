@echo off
chcp 65001 > nul
title CI/CD Demo - QuanLyKhoaHoc5

echo.
echo ================================================================
echo   CI/CD DEMO - GITHUB ACTIONS
echo   Project : QuanLyKhoaHoc5 (ASP.NET Core MVC)
echo   Remote  : https://github.com/xuanbac9968-hue/QuanLyKhoaHoc5
echo ================================================================
echo.

:: ── Chuyen ve thu muc goc du an ─────────────────────────────────
cd /d "%~dp0.."

echo [BUOC 1] Kiem tra Git remote...
git remote -v
if %ERRORLEVEL% NEQ 0 (
    echo [LOI] Khong tim thay Git remote!
    echo       Thiet lap remote bang lenh:
    echo       git remote add origin https://github.com/xuanbac9968-hue/QuanLyKhoaHoc5.git
    pause & exit /b 1
)
echo.

echo [BUOC 2] Kiem tra trang thai Git...
git status
echo.

echo [BUOC 3] Them tat ca file thay doi vao stage...
git add -A
echo [OK] Da them tat ca file vao stage.
echo.

echo [BUOC 4] Commit voi message CI/CD trigger...
git commit -m "ci: Add GitHub Actions CI/CD pipeline + unit tests coverage 82.9%%"
if %ERRORLEVEL% NEQ 0 (
    echo [INFO] Khong co gi de commit hoac da commit roi. Tiep tuc push...
)
echo.

echo [BUOC 5] Push len GitHub (main branch)...
echo.
echo  Dang push... (co the mat 10-30 giay)
echo.
git push origin main
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [!] Push that bai. Thu dung lenh sau neu can:
    echo     git push origin main --force
    echo.
    echo Hoac kiem tra lai credentials:
    echo     git config --global credential.helper manager
    pause & exit /b 1
)
echo.
echo ================================================================
echo  [OK] PUSH THANH CONG!
echo ================================================================
echo.

echo [BUOC 6] Mo GitHub Actions trong trinh duyet...
echo.
echo  GitHub Actions se tu dong chay pipeline khi detect push moi.
echo  Pipeline gom 5 jobs:
echo    1. build       - Kiem tra build du an
echo    2. unit-tests  - Chay 531 unit tests + coverage report
echo    3. sonarqube   - Kiem tra chat luong code (SonarCloud)
echo    4. katalon     - Chay 6 test cases E2E (TC01-TC06)
echo    5. notify      - Bao cao ket qua tong hop
echo.
echo  Mo browser...
timeout /t 3 /nobreak > nul
start "" "https://github.com/xuanbac9968-hue/QuanLyKhoaHoc5/actions"

echo.
echo ================================================================
echo  HUONG DAN DEMO CHO CO GIAO:
echo.
echo  1. Tren GitHub Actions tab, tim workflow "CI/CD Pipeline"
echo     vua duoc trigger (vua push xong)
echo.
echo  2. Click vao workflow run do xem chi tiet
echo.
echo  3. Thay 5 jobs: build / unit-tests / sonarqube / katalon / notify
echo     - Moi job hien thi Pass (xanh) hoac Fail (do)
echo     - Click vao tung job de xem log chi tiet
echo.
echo  4. Tab "Summary" hien thi bang ket qua dep:
echo     - Build Status
echo     - Test Results (531 tests passed)
echo     - Coverage (82.9%%)
echo     - SonarQube Quality Gate
echo     - Katalon E2E Results (6/6 passed)
echo.
echo  5. Tab "Checks" o moi PR hien thi ket qua test truc tiep
echo.
echo ================================================================
echo.
echo Nhan phim bat ky de dong...
pause > nul
