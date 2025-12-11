# ExcelData 运行时加载器

这个模块提供运行时加载 ExcelData ScriptableObject 资源的功能。数据类和数据资源由 Python 工具生成。

## 功能特性

- ✅ 运行时数据读取管理器
- ✅ 自动发现并加载所有 ExcelData 表
- ✅ 支持懒加载模式（按需加载）
- ✅ 类型安全的数据访问

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
PlayerData playerData = ExcelDataLoader.GetTable<PlayerData>("PlayerData");
```

### 方法 2: 懒加载模式（推荐用于大型项目）

```csharp
using ExcelImporter;

// 初始化并开启懒加载模式
ExcelDataLoader.InitializeLazy();

// 表会在第一次访问时自动加载
var enemyData = ExcelDataLoader.GetTable<EnemyData>();
var heroData = ExcelDataLoader.GetTable<HeroData>();
```

### 方法 3: 使用 ExcelDataManager

```csharp
using ExcelImporter;
using ExcelData;

// 自动加载所有表
ExcelDataManager.LoadAllTables();

// 获取所有表
var allTables = ExcelDataManager.GetAllTables();

// 根据表名获取表
EnemyData data = ExcelDataManager.GetTable<EnemyData>("EnemyData");

// 访问数据
if (data != null)
{
    for (int i = 0; i < data.Count; i++)
    {
        EnemyDataRow row = data.GetRow(i);
        Debug.Log($"ID: {row.Id}, Name: {row.Name}");
    }
}
```

## 数据生成

数据类和 ScriptableObject 资源由 Python 工具生成，请参考 Python 工具的文档。

## 注意事项

1. 资源文件必须保存在 `Resources/ExcelData` 路径下
2. 确保数据类已正确生成在 `Scripts/ExcelData` 目录下
3. 使用懒加载模式可以减少启动时的加载时间
