#!/bin/bash

echo "=== Unity Excel/CSV Importer Tool (Python) ==="
echo ""

cd "$(dirname "$0")"

python3 import_csv.py

if [ $? -ne 0 ]; then
    echo ""
    echo "错误: Python 未安装或脚本执行失败"
    echo "请确保已安装 Python 3.6 或更高版本"
fi

echo ""
echo "Press Enter to exit..."
read
