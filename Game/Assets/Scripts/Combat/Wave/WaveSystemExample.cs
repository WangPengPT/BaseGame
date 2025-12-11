using UnityEngine;
using ExcelImporter;
using ExcelData;
using System.Linq;
using Game.Combat.Wave;

/// <summary>
/// Wave系统接入示例
/// 展示如何读取和使用Wave数据
/// </summary>
public class WaveSystemExample : MonoBehaviour
{
    [Header("引用")]
    public WaveManager WaveManager;

    void Start()
    {
        // 1. 确保数据已加载
        ExcelDataLoader.Initialize();

        // 2. 读取Wave数据示例
        ReadWaveDataExample();

        // 3. 如果WaveManager已配置，可以监听事件
        if (WaveManager != null)
        {
            SetupWaveManagerEvents();
        }
    }

    /// <summary>
    /// 示例：如何读取Wave数据
    /// </summary>
    void ReadWaveDataExample()
    {
        // 获取WaveData表
        var waveDataTable = ExcelDataLoader.GetTable<WaveData>();
        if (waveDataTable == null)
        {
            Debug.LogWarning("WaveData表未找到");
            return;
        }

        Debug.Log($"共有 {waveDataTable.Count} 个关卡");

        // 遍历所有关卡
        foreach (var wave in waveDataTable.rows)
        {
            Debug.Log($"关卡 {wave.WaveIndex}: {wave.Name} " +
                     $"(Boss: {wave.IsBossWave}, 难度: {wave.DifficultyScalar})");

            // 获取该关卡的所有敌人条目
            var entryTable = ExcelDataLoader.GetTable<WaveEntryData>();
            var entries = entryTable?.rows.Where(e => e.WaveId == wave.Id).ToList();

            if (entries != null && entries.Count > 0)
            {
                Debug.Log($"  包含 {entries.Count} 个生成组:");
                foreach (var entry in entries)
                {
                    Debug.Log($"    - 敌人ID: {entry.EnemyId}, 数量: {entry.Count}, " +
                             $"模式: {entry.SpawnPattern}, 延迟: {entry.SpawnDelay}s");
                }
            }

            // 获取奖励数据
            if (!string.IsNullOrEmpty(wave.RewardTableId))
            {
                var rewardTable = ExcelDataLoader.GetTable<WaveRewardData>();
                var reward = rewardTable?.rows.FirstOrDefault(r => r.RewardTableId == wave.RewardTableId);
                if (reward != null)
                {
                    Debug.Log($"  奖励: 金币 {reward.GoldMin}-{reward.GoldMax}, " +
                             $"稀有度加成: {reward.RarityBias}");
                }
            }
        }
    }

    /// <summary>
    /// 示例：如何根据WaveIndex获取关卡数据
    /// </summary>
    public WaveDataRow GetWaveByIndex(int waveIndex)
    {
        var table = ExcelDataLoader.GetTable<WaveData>();
        return table?.rows.FirstOrDefault(w => w.WaveIndex == waveIndex);
    }

    /// <summary>
    /// 示例：如何获取某个关卡的所有敌人条目
    /// </summary>
    public System.Collections.Generic.List<WaveEntryDataRow> GetWaveEntries(int waveId)
    {
        var table = ExcelDataLoader.GetTable<WaveEntryData>();
        return table?.rows.Where(e => e.WaveId == waveId).ToList();
    }

    /// <summary>
    /// 示例：如何获取奖励数据
    /// </summary>
    public WaveRewardDataRow GetRewardData(string rewardTableId)
    {
        var table = ExcelDataLoader.GetTable<WaveRewardData>();
        return table?.rows.FirstOrDefault(r => r.RewardTableId == rewardTableId);
    }

    /// <summary>
    /// 设置WaveManager事件监听
    /// </summary>
    void SetupWaveManagerEvents()
    {
        WaveManager.OnWaveStarted += OnWaveStarted;
        WaveManager.OnWaveCompleted += OnWaveCompleted;
        WaveManager.OnRewardsDropped += OnRewardsDropped;
    }

    void OnDestroy()
    {
        if (WaveManager != null)
        {
            WaveManager.OnWaveStarted -= OnWaveStarted;
            WaveManager.OnWaveCompleted -= OnWaveCompleted;
            WaveManager.OnRewardsDropped -= OnRewardsDropped;
        }
    }

    private void OnWaveStarted(int waveIndex)
    {
        Debug.Log($"关卡 {waveIndex} 开始！");
        
        // 可以在这里显示UI提示
        var waveData = GetWaveByIndex(waveIndex);
        if (waveData != null)
        {
            Debug.Log($"关卡名称: {waveData.Name}");
            if (waveData.IsBossWave)
            {
                Debug.Log("⚠️ BOSS关卡！");
            }
        }
    }

    private void OnWaveCompleted(int waveIndex)
    {
        Debug.Log($"关卡 {waveIndex} 完成！");
    }

    private void OnRewardsDropped(System.Collections.Generic.List<RewardItem> rewards)
    {
        Debug.Log($"获得奖励:");
        foreach (var reward in rewards)
        {
            Debug.Log($"  - {reward.Type}: {reward.Amount}");
        }
    }

    /// <summary>
    /// 示例：手动启动指定关卡（可用于测试或跳关）
    /// </summary>
    [ContextMenu("测试：启动第1关")]
    public void TestStartWave1()
    {
        if (WaveManager != null)
        {
            WaveManager.CurrentWaveIndex = 1;
            // 注意：这需要根据WaveManager的实际实现调整
        }
    }
}

