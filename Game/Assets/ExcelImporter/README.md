# Excel 导入工具

这个工具可以将 Excel 文件（.xlsx, .xls）或 CSV 文件导入到 Unity 项目中，自动生成 C# 类定义和 ScriptableObject 资源。

## 功能特性

- ✅ 支持 Excel (.xlsx, .xls) 和 CSV 文件格式
- ✅ 自动生成 C# 数据类
- ✅ 自动生成 ScriptableObject 资源
- ✅ 自动推断数据类型（int, float, bool, string）
- ✅ 运行时数据读取管理器

## 使用方法

### 1. 准备 Excel 文件

将 Excel 或 CSV 文件放在 `Document` 目录下。文件格式要求：
- 第一行必须是字段名（表头）
- 第二行开始是数据
- 字段名会自动转换为 C# 合法的字段名（PascalCase）
- **类名将自动使用文件名（去掉扩展名）**

示例文件：
- `示例数据.csv` → 类名：`示例数据`
- `PlayerData.xlsx` → 类名：`PlayerData`
- `物品配置.csv` → 类名：`物品配置`

### 2. 打开导入工具

在 Unity 编辑器中，点击菜单：`Tools > Excel Importer`

### 3. 配置导入选项

工具会自动扫描 `Document` 目录下的所有 Excel (.xlsx, .xls) 和 CSV 文件。

- **Document 目录**: Document 文件夹路径（默认自动检测）
- **资源输出路径**: ScriptableObject 资源保存路径（相对于 Assets）
- **脚本输出路径**: C# 脚本保存路径（相对于 Assets）
- **生成数据类**: 是否生成数据类脚本
- **生成 ScriptableObject**: 是否生成 ScriptableObject 资源

### 4. 执行批量导入

点击"导入所有文件"按钮，工具会：
1. 自动扫描 Document 目录下的所有文件
2. 为每个文件生成对应的 C# 类（类名 = 文件名）
3. 生成 ScriptableObject 资源（如果启用）
4. 显示导入结果（成功/失败的文件列表）

## 运行时读取数据

### 方法 1: 自动加载所有表（推荐）⭐

**无需手动编写每个表的加载代码！** 工具会自动发现并加载所有表。

```csharp
using ExcelImporter;

// 初始化并加载所有表
ExcelDataLoader.Initialize();

// 获取所有表
var allTables = ExcelDataLoader.GetAllTables();
foreach (var kvp in allTables)
{
    Debug.Log($"表名: {kvp.Key}, 行数: {kvp.Value.Count}");
}

// 根据表名获取特定表（使用接口，通用方式）
IExcelDataTable table = ExcelDataLoader.GetTable("PlayerData");
if (table != null)
{
    Debug.Log($"表 {table.TableName} 有 {table.Count} 行");
}

// 根据表名获取特定表（使用泛型，类型安全）
// PlayerData playerData = ExcelDataLoader.GetTable<PlayerData>("PlayerData");
```

### 方法 2: 使用 ExcelDataManager

```csharp
using ExcelImporter;
using ExcelData;

// 自动加载所有表
ExcelDataManager.LoadAllTables();

// 获取所有表
var allTables = ExcelDataManager.GetAllTables();

// 根据表名获取表
PlayerData data = ExcelDataManager.GetTable<PlayerData>("PlayerData");

// 访问数据
if (data != null)
{
    for (int i = 0; i < data.Count; i++)
    {
        PlayerDataRow row = data.GetRow(i);
        Debug.Log($"ID: {row.Id}, Name: {row.Name}, Age: {row.Age}");
    }
}
```

### 方法 3: 直接使用 Resources.Load（不推荐）

```csharp
using ExcelData;

PlayerData data = Resources.Load<PlayerData>("PlayerData");
if (data != null)
{
    foreach (var row in data.rows)
    {
        Debug.Log($"Name: {row.Name}, Score: {row.Score}");
    }
}
```

## 支持的文件格式

### CSV 格式（推荐）

CSV 格式最简单，不需要任何额外依赖。只需将 Excel 文件另存为 CSV 格式即可。

### Excel 格式

支持两种方式读取 Excel：

1. **System.Data.OleDb**（默认）
   - 需要安装 Microsoft Access Database Engine
   - 下载地址：https://www.microsoft.com/en-us/download/details.aspx?id=54920

2. **EPPlus**（可选）
   - 需要安装 EPPlus.Core 库
   - 通过 NuGet 或手动添加 DLL

## 生成的文件结构

导入后会生成以下文件：

```
Assets/
  ExcelData/
    Scripts/
      PlayerData.cs          # 数据类定义
    Resources/
      PlayerData.asset       # ScriptableObject 资源
```

## 数据类型推断规则

工具会根据数据内容自动推断字段类型：

- **bool**: 值为 "true"/"false"/"1"/"0"/"yes"/"no"
- **int**: 所有值都是整数
- **float**: 所有值都是数字（包含小数）
- **string**: 其他情况

## 注意事项

1. Excel 文件的第一行必须是字段名
2. 字段名中的空格和特殊字符会被自动处理
3. 如果字段名是 C# 关键字，会自动添加 @ 前缀
4. 空行会被跳过
5. 资源文件会保存在指定的 Resources 路径下，以便运行时加载

## 故障排除

### 无法读取 Excel 文件

如果遇到 "无法使用 OleDb 读取 Excel" 错误：
1. 安装 Microsoft Access Database Engine
2. 或者将 Excel 文件导出为 CSV 格式

### 生成的类找不到

如果编译后找不到生成的类：
1. 确保脚本已保存
2. 等待 Unity 编译完成
3. 检查命名空间是否正确（默认：ExcelData）

### 资源加载失败

如果运行时无法加载资源：
1. 确保资源文件在 Resources 文件夹下
2. 检查资源路径是否正确
3. 确保资源文件已正确生成

