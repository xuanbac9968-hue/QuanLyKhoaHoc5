@echo off
chcp 65001 > nul
title Reset DB - QuanLyKhoaHoc5

echo.
echo ==============================================
echo   RESET DB - Xoa tai khoan test TC03
echo   Database: NgoaiNguABC  /  Server: .
echo ==============================================
echo.

set SERVER=.
set DATABASE=NgoaiNguABC

:: Kiem tra sqlcmd co san khong
where sqlcmd > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [LOI] Khong tim thay sqlcmd.exe
    echo       Cai dat SQL Server hoac them vao PATH:
    echo       C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\...\Tools\Binn\
    echo.
    pause
    exit /b 1
)

echo [1] Tai khoan test hien tai (truoc khi xoa):
echo -----------------------------------------------
sqlcmd -S %SERVER% -d %DATABASE% -E -W -Q ^
    "SET NOCOUNT OFF; SELECT Email, VaiTro FROM NguoiDungs WHERE Email LIKE '%%hvmoi%%' OR Email LIKE '%%gvmoi%%' OR Email LIKE '%%admin_moi%%' ORDER BY VaiTro, Email;"
echo.

echo [2] Xoa DangKyKhoaHocs cua cac HocVien test (neu co)...
sqlcmd -S %SERVER% -d %DATABASE% -E -Q ^
    "DELETE FROM DangKyKhoaHocs WHERE HocVienId IN (SELECT Id FROM NguoiDungs WHERE Email LIKE '%%hvmoi%%');"
echo     Done.

echo [3] Xoa ThanhToans cua cac HocVien test (neu co)...
sqlcmd -S %SERVER% -d %DATABASE% -E -Q ^
    "DELETE FROM ThanhToans WHERE HocVienId IN (SELECT Id FROM NguoiDungs WHERE Email LIKE '%%hvmoi%%');"
echo     Done.

echo [4] Xoa tai khoan test (hvmoi / gvmoi / admin_moi)...
sqlcmd -S %SERVER% -d %DATABASE% -E -Q ^
    "DELETE FROM NguoiDungs WHERE Email LIKE '%%hvmoi%%' OR Email LIKE '%%gvmoi%%' OR Email LIKE '%%admin_moi%%';"

if %ERRORLEVEL% EQU 0 (
    echo     Done.
) else (
    echo.
    echo [LOI] Xoa that bai. Co the do:
    echo       - SQL Server chua chay
    echo       - Database NgoaiNguABC khong ton tai
    echo       - Khong co quyen truy cap
    echo.
    pause
    exit /b 1
)

echo.
echo [5] Ket qua sau reset:
echo -----------------------------------------------
sqlcmd -S %SERVER% -d %DATABASE% -E -W -Q ^
    "SET NOCOUNT OFF; SELECT COUNT(*) AS [Con_lai_sau_reset] FROM NguoiDungs WHERE Email LIKE '%%hvmoi%%' OR Email LIKE '%%gvmoi%%' OR Email LIKE '%%admin_moi%%';"

echo.
echo ==============================================
echo   XONG! Co the chay Katalon TS_All.
echo ==============================================
echo.
pause
