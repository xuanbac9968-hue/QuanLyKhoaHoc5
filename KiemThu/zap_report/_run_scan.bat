@echo off
set JAVA="C:\Program Files\Eclipse Adoptium\jdk-25.0.3.9-hotspot\bin\java.exe"
set JAR="C:\Program Files\ZAP\Zed Attack Proxy\zap-2.17.0.jar"
set PLAN="D:\QuanLyKhoaHoc5\KiemThu\zap_plan.yaml"

%JAVA% -Xmx768m -jar %JAR% -cmd -autorun %PLAN% -port 8090
