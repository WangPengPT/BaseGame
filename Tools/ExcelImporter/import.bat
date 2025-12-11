@echo off
echo === Unity Excel/CSV Importer Tool (Python) ===
echo.

cd /d "%~dp0"

python import_csv.py

if errorlevel 1 (
    echo.
    echo 错误: Python 未安装或脚本执行失败
    echo 请确保已安装 Python 3.6 或更高版本
)

echo.
echo Press any key to exit...
pause >nul
