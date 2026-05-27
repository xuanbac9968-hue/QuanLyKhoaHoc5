@echo off
setlocal EnableDelayedExpansion

REM ============================================================
REM  chay_coverage.bat
REM  Tự động chạy toàn bộ pipeline: build → test + coverage
REM  → SonarQube scan → hiện kết quả.
REM
REM  Yêu cầu:
REM    - .NET SDK 10 (hoặc tương thích)
REM    - dotnet-sonarscanner đã cài (dotnet tool install -g dotnet-sonarscanner)
REM    - SonarQube server đang chạy tại http://localhost:9000
REM  Cách dùng: Chạy file này từ thư mục gốc dự án hoặc double-click.
REM ============================================================

REM Đặt thư mục làm việc về gốc dự án (cha của KiemThu)
cd /d "%~dp0.."

echo.
echo ===================================================
echo  BUOC 1: Kiem tra cong cu
echo ===================================================
where dotnet >nul 2>&1
if errorlevel 1 (
    echo [LOI] Khong tim thay .NET SDK. Vui long cai dat .NET SDK 10.
    pause & exit /b 1
)
dotnet sonarscanner --version >nul 2>&1
if errorlevel 1 (
    echo [LOI] Khong tim thay dotnet-sonarscanner.
    echo       Cai dat bang lenh: dotnet tool install -g dotnet-sonarscanner
    pause & exit /b 1
)
echo [OK] .NET SDK va SonarScanner da san sang.

echo.
echo ===================================================
echo  BUOC 2: SonarQube Begin
echo ===================================================
dotnet sonarscanner begin ^
  /k:"QuanLyKhoaHoc5" ^
  /d:sonar.host.url="http://localhost:9000" ^
  /d:sonar.token="sqa_d40db0e27f23e9c1f98654eb5a80fdfde94c4c9c" ^
  /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml" ^
  /d:"sonar.exclusions=KiemThu/**,**/wwwroot/**,**/Migrations/**,**/obj/**,**/bin/**"
if errorlevel 1 (
    echo [LOI] SonarQube Begin that bai.
    pause & exit /b 1
)

echo.
echo ===================================================
echo  BUOC 3: Build du an Web
echo ===================================================
dotnet build QuanLyKhoaHoc5.Web\QuanLyKhoaHoc5.Web.csproj --configuration Release
if errorlevel 1 (
    echo [LOI] Build that bai.
    pause & exit /b 1
)

echo.
echo ===================================================
echo  BUOC 4: Chay Tests + Thu thap Coverage
echo ===================================================
dotnet test QuanLyKhoaHoc5.Tests\QuanLyKhoaHoc5.Tests.csproj ^
  --configuration Release ^
  --collect:"XPlat Code Coverage" ^
  --results-directory ./TestResults ^
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
if errorlevel 1 (
    echo [CANH BAO] Mot so test that bai. Ket qua coverage co the bi anh huong.
    echo             Tiep tuc chay SonarQube...
)

echo.
echo ===================================================
echo  BUOC 5: SonarQube End (upload ket qua)
echo ===================================================
dotnet sonarscanner end ^
  /d:sonar.token="sqa_d40db0e27f23e9c1f98654eb5a80fdfde94c4c9c"
if errorlevel 1 (
    echo [LOI] SonarQube End that bai.
    pause & exit /b 1
)

echo.
echo ===================================================
echo  BUOC 6: Cho SonarQube xu ly (15 giay)...
echo ===================================================
timeout /t 15 /nobreak >nul

echo.
echo ===================================================
echo  BUOC 7: Ket qua Coverage tu SonarQube API
echo ===================================================
curl -s -u "sqa_d40db0e27f23e9c1f98654eb5a80fdfde94c4c9c:" ^
  "http://localhost:9000/api/measures/component?component=QuanLyKhoaHoc5&metricKeys=coverage,lines_to_cover,uncovered_lines" ^
  2>nul | findstr /i "coverage\|value\|metric"

echo.
echo ===================================================
echo  HOAN THANH!
echo  Mo trinh duyet tai: http://localhost:9000/dashboard?id=QuanLyKhoaHoc5
echo ===================================================
echo.

REM Mo dashboard trong trinh duyet mac dinh
start "" "http://localhost:9000/dashboard?id=QuanLyKhoaHoc5"

pause
