# Wave 关卡系统使用指南

## 概述

Wave系统用于管理游戏中的关卡流程，包括敌人生成、关卡进度和奖励发放。

## 数据结构

### WaveData (关卡数据)
- `Id`: 关卡ID
- `Name`: 关卡名称
- `WaveIndex`: 关卡序号（1, 2, 3...）
- `IsBossWave`: 是否为Boss关卡
- `DifficultyScalar`: 难度系数（影响敌人血量等）
- `PrepTime`: 准备时间（秒）
- `AutoStart`: 是否自动开始
- `RewardTableId`: 奖励表ID
- `SpawnPointGroupName`: 生成点组GameObject名称（场景中需有同名GameObject）
- `CenterPointName`: 中心点GameObject名称（用于circle/arc模式）

### WaveEntryData (关卡敌人条目)
- `Id`: 条目ID
- `WaveId`: 所属关卡ID
- `EnemyId`: 敌人ID
- `Count`: 生成数量
- `SpawnPattern`: 生成模式（circle/arc/center）
- `SpawnDelay`: 生成延迟（秒）
- `IsElite`: 是否为精英敌人
- `AffixTags`: 词缀标签

### WaveRewardData (关卡奖励数据)
- `Id`: 奖励ID
- `RewardTableId`: 奖励表ID
- `RarityBias`: 稀有度加成
- `GoldMin/GoldMax`: 金币范围
- `ItemPoolId`: 物品池ID
- `ScoreToLootScalar`: 分数转战利品系数

## 快速接入

### 1. 在Excel中配置Wave数据

所有配置都在 `Document/Config/WaveData.csv` 中完成，无需在Unity Inspector中手动设置：

**WaveData.csv 字段说明：**
- `spawn_point_group_name`: 生成点组GameObject名称（场景中需要有一个同名GameObject，其子对象作为生成点）
- `center_point_name`: 中心点GameObject名称（用于circle/arc模式，场景中需要有同名GameObject）

**注意：** 敌人预制体路径现在在 `EnemyData.csv` 中配置，每个敌人/英雄都有自己的 `prefab_path` 字段。

**示例配置：**
```csv
id,name,wave_index,spawn_point_group_name,center_point_name
1,Wave 1,1,SpawnPoints,CenterPoint
```

### 2. 在场景中设置GameObject

**必须的GameObject：**
1. **生成点组**：创建一个名为 `SpawnPoints` 的GameObject（名称需与Excel中配置一致）
   - 在该GameObject下创建多个子对象作为生成点
   - 子对象的位置将作为敌人生成位置

2. **中心点**：创建一个名为 `CenterPoint` 的GameObject（名称需与Excel中配置一致）
   - 用于circle和arc模式的中心参考点

3. **敌人预制体**：将敌人预制体放在 `Resources/` 文件夹下
   - 每个敌人的预制体路径在 `EnemyData.csv` 中的 `prefab_path` 字段配置
   - 例如：`Enemies/FrostImp` 表示 `Resources/Enemies/FrostImp.prefab`

### 3. 在场景中添加WaveManager组件

```csharp
// 1. 在场景中添加WaveManager组件到任意GameObject
// 2. 只需设置 CurrentWaveIndex（起始关卡序号，通常为1）
// 3. 其他所有配置都从Excel数据中自动读取
```

### 4. 读取Wave数据

```csharp
using ExcelImporter;
using ExcelData;

// 确保数据已加载
ExcelDataLoader.Initialize();

// 获取WaveData表
var waveTable = ExcelDataLoader.GetTable<WaveData>();

// 根据WaveIndex获取关卡
var wave = waveTable.rows.FirstOrDefault(w => w.WaveIndex == 1);

// 获取该关卡的所有敌人条目
var entryTable = ExcelDataLoader.GetTable<WaveEntryData>();
var entries = entryTable.rows.Where(e => e.WaveId == wave.Id).ToList();

// 获取奖励数据
var rewardTable = ExcelDataLoader.GetTable<WaveRewardData>();
var reward = rewardTable.rows.FirstOrDefault(r => r.RewardTableId == wave.RewardTableId);
```

### 3. 监听Wave事件

```csharp
using Game.Combat.Wave;

WaveManager waveManager = GetComponent<WaveManager>();

// 监听关卡开始
waveManager.OnWaveStarted += (waveIndex) => {
    Debug.Log($"关卡 {waveIndex} 开始");
};

// 监听关卡完成
waveManager.OnWaveCompleted += (waveIndex) => {
    Debug.Log($"关卡 {waveIndex} 完成");
};

// 监听奖励掉落
waveManager.OnRewardsDropped += (rewards) => {
    foreach (var reward in rewards)
    {
        Debug.Log($"获得: {reward.Type} x{reward.Amount}");
    }
};
```

## 生成模式 (Spawn Pattern)

### circle (圆形)
敌人围绕中心点均匀分布成圆形。

```csharp
// 在WaveEntryData中设置 SpawnPattern = "circle"
// 需要设置CenterPoint
```

### arc (弧形)
敌人在指定角度范围内形成弧形分布。

```csharp
// 在WaveEntryData中设置 SpawnPattern = "arc"
// 默认120度弧形，从-60度到+60度
```

### center (中心)
敌人在中心点位置生成（通常用于Boss）。

```csharp
// 在WaveEntryData中设置 SpawnPattern = "center"
```

## 完整示例

参考 `WaveSystemExample.cs` 文件，包含：
- 如何读取所有Wave数据
- 如何根据条件查询数据
- 如何监听Wave事件
- 如何获取奖励信息

## 工作流程

1. **准备阶段 (Prep Phase)**
   - 显示倒计时

2. **关卡阶段 (Wave Phase)**
   - 根据WaveEntryData生成敌人
   - 应用难度系数和精英加成
   - 等待所有敌人被击败

3. **奖励阶段 (Reward Phase)**
   - 根据WaveRewardData生成奖励
   - 掉落奖励物品

4. **下一关**
   - 自动进入下一关（如果AutoStart为true）
   - 或等待玩家手动开始

## 注意事项

1. **数据加载顺序**
   - 确保在Start()中调用 `ExcelDataLoader.Initialize()`
   - WaveManager会自动初始化CombatDataRegistry
   - 修改Excel后需要重新运行导入工具

2. **场景GameObject命名**
   - 生成点组GameObject名称必须与Excel中 `spawn_point_group_name` 完全一致
   - 中心点GameObject名称必须与Excel中 `center_point_name` 完全一致
   - 使用 `GameObject.Find()` 查找，区分大小写

3. **生成点组结构**
   - 生成点组GameObject下的所有子对象都会作为生成点
   - 如果生成点组为空，会显示警告

4. **敌人预制体路径**
   - 预制体必须放在 `Resources` 文件夹下
   - 路径格式：`"Enemies/EnemyPrefab"`（不需要扩展名）
   - 路径在 `EnemyData.csv` 中的 `prefab_path` 字段配置，每个敌人/英雄都有自己的预制体路径

5. **敌人预制体要求**
   - 必须包含 `CombatActor` 组件
   - 可选包含 `CombatAIController` 组件

6. **难度系数**
   - DifficultyScalar会影响敌人的基础血量
   - Boss关卡通常有更高的难度系数

7. **自动配置**
   - PrepTime、AutoStart等参数都从Excel中读取
   - 每个关卡可以有不同的配置

