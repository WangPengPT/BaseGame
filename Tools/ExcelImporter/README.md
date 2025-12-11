# Unity Excel/CSV 导入工具（Python 版）

这是一个独立的 Python 命令行工具，可以在 Unity 外部运行，避免 Unity 编译冲突问题。

## 系统要求

- Python 3.6 或更高版本
- 无需安装额外依赖（使用标准库）

## 使用方法

### Windows

双击 `import.bat` 即可运行。

### Mac/Linux

```bash
chmod +x import.sh
./import.sh
```

### 手动运行

```bash
cd Tools/ExcelImporter
python import_csv.py
```

### 自定义参数

```bash
python import_csv.py [Document路径] [Unity项目路径] [脚本输出路径] [资源输出路径]

# 示例：
python import_csv.py "../../Document/Config" "../../Game" "Assets/Scripts/ExcelData" "Assets/Resources/ExcelData"
```

## 功能特性

- ✅ 独立运行，不依赖 Unity Editor
- ✅ 使用 Python，无需安装特定版本的 .NET
- ✅ 读取 CSV 文件
- ✅ 自动生成 C# 类文件
- ✅ 自动生成 Unity ScriptableObject 资源文件（.asset）
- ✅ 自动类型推断（int, float, bool, string）
- ✅ 自动生成 GetById 方法
- ✅ 直接写入 Unity Assets 目录

## 工作流程

1. 工具扫描 `Document/Config` 目录下的所有 CSV 文件
2. 读取 CSV 文件，解析表头和数据
3. 生成 C# 脚本文件到 `Assets/Scripts/ExcelData`
4. 生成 Unity 资源文件到 `Assets/Resources/ExcelData`
5. Unity 会自动检测到新文件并重新导入

## 注意事项

- 确保 CSV 文件第一行是表头
- 工具会自动推断字段类型
- 如果字段名包含 "id"（不区分大小写），会自动生成 GetById 方法
- 生成的资源文件需要 Unity 重新导入才能使用
- CSV 文件应使用 UTF-8 编码（带或不带 BOM 都可以）

## 故障排除

### Python 未找到

如果提示 "Python 未安装"，请：
1. 访问 https://www.python.org/downloads/ 下载安装 Python
2. 安装时勾选 "Add Python to PATH"
3. 重新运行脚本

### 编码问题

如果 CSV 文件包含中文乱码，请确保文件使用 UTF-8 编码保存。
