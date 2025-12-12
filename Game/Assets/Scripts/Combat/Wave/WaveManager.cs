using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Combat;
using ExcelImporter;
using ExcelData;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Combat.Wave
{
    /// <summary>
    /// Manages wave-based combat: spawns enemies per wave, detects completion, drops rewards.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Settings")]
        public int CurrentWaveIndex = 0;
        // 注意：以下配置现在从Excel数据中读取，无需在Inspector中设置
        // PrepTime, AutoStart, EarlyStartBonus 等将从WaveData中读取

        [Header("Reward Settings")]
        public Transform RewardDropPoint; // 奖励掉落点（可选，如果为空则使用中心点）
        public GameObject GoldPrefab; // 金币预制体（可选）
        public GameObject ItemPrefab; // 物品预制体（可选）

        private List<CombatActor> _activeEnemies = new List<CombatActor>();
        private bool _isPreparing = false;
        private bool _isWaveActive = false;
        private float _prepTimer = 0f;
        private float _waveStartTime = 0f;
        private bool _earlyStartUsed = false;

        // 当前关卡的动态配置（从数据中加载）
        private Transform[] _currentSpawnPoints;
        private Transform _currentCenterPoint;

        public System.Action<int> OnWaveStarted;
        public System.Action<int> OnWaveCompleted;
        public System.Action<float> OnPrepTimerChanged;
        public System.Action<List<RewardItem>> OnRewardsDropped;

        private void Start()
        {
            if (CurrentWaveIndex < 1) CurrentWaveIndex = 1;
            CombatDataRegistry.Initialize(lazy: true);
            StartCoroutine(WaveLoop());
        }

        private IEnumerator WaveLoop()
        {
            while (true)
            {
                // 检查当前波次是否存在
                var waveData = GetWaveData(CurrentWaveIndex);
                if (waveData == null)
                {
                    Debug.Log($"Wave {CurrentWaveIndex} 不存在，关卡结束");
                    break; // 没有更多波次，结束循环
                }

                // Prep phase
                yield return StartCoroutine(PrepPhase());

                // Wave phase - 等待所有敌人生成并清理完成
                yield return StartCoroutine(WavePhase());

                // Reward phase
                yield return StartCoroutine(RewardPhase());
            }
        }

        private IEnumerator PrepPhase()
        {
            var waveData = GetWaveData(CurrentWaveIndex);
            if (waveData == null)
            {
                Debug.LogWarning($"Wave {CurrentWaveIndex} not found in data");
                yield break;
            }

            // 从数据中加载当前关卡的配置
            LoadWaveConfig(waveData);

            _isPreparing = true;
            _prepTimer = waveData.PrepTime;
            _earlyStartUsed = false;

            OnWaveStarted?.Invoke(CurrentWaveIndex);

            // 如果AutoStart为false，等待玩家手动开始
            if (!waveData.AutoStart)
            {
                while (_prepTimer > 0f)
                {
                    _prepTimer -= Time.deltaTime;
                    OnPrepTimerChanged?.Invoke(_prepTimer);

                    // Check for early start (player can press a key or button)
                    if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !_earlyStartUsed)
                    {
                        _earlyStartUsed = true;
                        break; // Start wave early
                    }

                    yield return null;
                }
            }
            else
            {
                // AutoStart为true时，仍然允许提前开始
                while (_prepTimer > 0f)
                {
                    _prepTimer -= Time.deltaTime;
                    OnPrepTimerChanged?.Invoke(_prepTimer);

                    if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !_earlyStartUsed)
                    {
                        _earlyStartUsed = true;
                        break;
                    }

                    yield return null;
                }
            }

            _isPreparing = false;
        }

        private IEnumerator WavePhase()
        {
            _isWaveActive = true;
            _waveStartTime = Time.time;

            var waveData = GetWaveData(CurrentWaveIndex);
            if (waveData == null)
            {
                Debug.LogWarning($"Wave {CurrentWaveIndex} not found in data");
                _isWaveActive = false;
                yield break;
            }

            // 清空上一波的敌人列表，确保从干净状态开始
            _activeEnemies.Clear();

            // Apply difficulty scalar
            float difficulty = waveData.DifficultyScalar;

            // Spawn enemies - 等待所有敌人生成完成
            var entries = GetWaveEntries(waveData.Id);
            foreach (var entry in entries)
            {
                yield return new WaitForSeconds(entry.SpawnDelay);

                for (int i = 0; i < entry.Count; i++)
                {
                    SpawnEnemy(entry.EnemyId, entry.IsElite, difficulty, entry.SpawnPattern, entry.Count, i);
                    yield return new WaitForSeconds(0.2f); // Small delay between spawns
                }
            }

            // 等待所有敌人被击败 - 确保一波的怪全部清理完成才进入下一波
            while (_activeEnemies.Count > 0)
            {
                // 清理已死亡或已销毁的敌人
                _activeEnemies.RemoveAll(e => e == null || e.IsDead);
                yield return null; // 每帧检查一次，更及时响应
            }

            _isWaveActive = false;
            OnWaveCompleted?.Invoke(CurrentWaveIndex);
        }

        private IEnumerator RewardPhase()
        {
            var waveData = GetWaveData(CurrentWaveIndex);
            if (waveData == null) yield break;

            var rewards = GenerateRewards(waveData.RewardTableId);
            DropRewards(rewards);

            OnRewardsDropped?.Invoke(rewards);

            yield return new WaitForSeconds(2f); // Brief pause before next wave

            CurrentWaveIndex++;
        }

        /// <summary>
        /// 获取当前关卡的奖励掉落点（如果未设置则使用中心点）
        /// </summary>
        private Transform GetRewardDropPoint()
        {
            if (RewardDropPoint != null)
                return RewardDropPoint;
            
            if (_currentCenterPoint != null)
                return _currentCenterPoint;
            
            return transform; // 最后使用WaveManager自身的位置
        }

        /// <summary>
        /// 从WaveData中加载当前关卡的配置（生成点、中心点等）
        /// 注意：敌人预制体路径现在从EnemyData中读取，不再从WaveData中读取
        /// </summary>
        private void LoadWaveConfig(WaveDataRow waveData)
        {
            // 加载生成点组
            if (!string.IsNullOrEmpty(waveData.SpawnPointGroupName))
            {
                GameObject spawnGroup = GameObject.Find(waveData.SpawnPointGroupName);
                if (spawnGroup != null)
                {
                    var spawnList = new List<Transform>();
                    foreach (Transform child in spawnGroup.transform)
                    {
                        spawnList.Add(child);
                    }
                    _currentSpawnPoints = spawnList.ToArray();
                }
                else
                {
                    Debug.LogWarning($"找不到生成点组: {waveData.SpawnPointGroupName}");
                    _currentSpawnPoints = new Transform[0];
                }
            }
            else
            {
                Debug.LogWarning($"Wave {waveData.WaveIndex} 未配置生成点组名称");
                _currentSpawnPoints = new Transform[0];
            }

            // 加载中心点
            if (!string.IsNullOrEmpty(waveData.CenterPointName))
            {
                GameObject centerObj = GameObject.Find(waveData.CenterPointName);
                _currentCenterPoint = centerObj != null ? centerObj.transform : null;
                if (_currentCenterPoint == null)
                {
                    Debug.LogWarning($"找不到中心点: {waveData.CenterPointName}");
                }
            }
        }

        private void SpawnEnemy(int enemyId, bool isElite, float difficultyScalar, string spawnPattern = "circle", int totalCount = 1, int spawnIndex = 0)
        {
            if (_currentSpawnPoints == null || _currentSpawnPoints.Length == 0)
            {
                Debug.LogWarning("生成点未正确加载，请检查Excel配置");
                return;
            }

            var enemyData = CombatDataRegistry.GetEnemy(enemyId);
            if (enemyData == null)
            {
                Debug.LogWarning($"Enemy {enemyId} not found");
                return;
            }

            // 从 EnemyData 中加载敌人预制体
            if (string.IsNullOrEmpty(enemyData.PrefabPath))
            {
                Debug.LogWarning($"Enemy {enemyId} ({enemyData.Name}) 未配置预制体路径");
                return;
            }

            GameObject enemyPrefab = Resources.Load<GameObject>(enemyData.PrefabPath);
            if (enemyPrefab == null)
            {
                Debug.LogWarning($"找不到敌人预制体: {enemyData.PrefabPath}，请确保预制体在Resources文件夹中");
                return;
            }

            // 根据生成模式计算位置
            Vector3 spawnPosition = SpawnPatternHelper.GetSpawnPosition(
                spawnPattern, 
                _currentSpawnPoints, 
                _currentCenterPoint, 
                totalCount, 
                spawnIndex
            );

            // 计算朝向（朝向中心点或玩家）
            Quaternion spawnRotation = Quaternion.identity;
            if (_currentCenterPoint != null)
            {
                Vector3 direction = (_currentCenterPoint.position - spawnPosition).normalized;
                if (direction != Vector3.zero)
                {
                    spawnRotation = Quaternion.LookRotation(direction);
                }
            }

            GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, spawnRotation);

            // Setup enemy actor
            var actor = enemyObj.GetComponent<CombatActor>();
            if (actor != null)
            {
                // Apply difficulty scaling
                actor.Stats.MaxHp = enemyData.BaseHp * difficultyScalar;
                actor.Stats.Hp = actor.Stats.MaxHp;
                actor.Stats.MaxMp = enemyData.BaseMp;
                actor.Stats.Mp = actor.Stats.MaxMp;
                actor.Stats.Armor = enemyData.Armor;

                // Apply resistances
                if (enemyData.ResistProfileId > 0)
                {
                    var resistProfile = GetResistProfile(enemyData.ResistProfileId);
                    if (resistProfile != null)
                    {
                        actor.Stats.Resist.IceResist = resistProfile.IceResist;
                        actor.Stats.Resist.FireResist = resistProfile.FireResist;
                        actor.Stats.Resist.LightningResist = resistProfile.LightningResist;
                        actor.Stats.Resist.PoisonResist = resistProfile.PoisonResist;
                    }
                }
                else
                {
                    // Apply direct resistances from enemy data if no profile
                    actor.Stats.Resist.PhysicalResist = enemyData.PhysicalResist;
                    actor.Stats.Resist.MagicalResist = enemyData.MagicalResist;
                    actor.Stats.Resist.IceResist = enemyData.IceResist;
                    actor.Stats.Resist.FireResist = enemyData.FireResist;
                    actor.Stats.Resist.LightningResist = enemyData.LightningResist;
                    actor.Stats.Resist.PoisonResist = enemyData.PoisonResist;
                }

                // Elite bonus
                if (isElite)
                {
                    actor.Stats.MaxHp *= 1.5f;
                    actor.Stats.Hp = actor.Stats.MaxHp;
                    actor.Stats.Armor *= 1.3f;
                }

                _activeEnemies.Add(actor);
            }

            // 添加移动控制器（如果不存在）
            var movementController = enemyObj.GetComponent<CombatMovementController>();
            if (movementController == null)
            {
                movementController = enemyObj.AddComponent<CombatMovementController>();
            }

            // 添加动画控制器（如果存在 Animator 组件）
            var animator = enemyObj.GetComponent<Animator>();
            if (animator != null)
            {
                var animationController = enemyObj.GetComponent<CombatAnimationController>();
                if (animationController == null)
                {
                    animationController = enemyObj.AddComponent<CombatAnimationController>();
                }
            }

            // Setup AI if available
            var ai = enemyObj.GetComponent<AI.CombatAIController>();
            if (ai != null && enemyData.BaseSkills > 0)
            {
                // BaseSkills is int in EnemyData - treat as single skill ID
                var config = GetAbilityConfig(enemyData.BaseSkills);
                if (config != null)
                {
                    ai.Abilities.Clear();
                    ai.Abilities.Add(config);
                }
            }
        }

        private WaveDataRow GetWaveData(int waveIndex)
        {
            var table = ExcelDataLoader.GetTable<WaveData>();
            return table?.rows.FirstOrDefault(r => r.WaveIndex == waveIndex);
        }

        private List<WaveEntryDataRow> GetWaveEntries(int waveId)
        {
            var table = ExcelDataLoader.GetTable<WaveEntryData>();
            if (table == null) return new List<WaveEntryDataRow>();
            
            // 过滤掉无效数据（Id为0或空的数据，可能是CSV空行导致的）
            return table.rows
                .Where(r => r != null && r.Id > 0 && r.WaveId == waveId)
                .ToList();
        }

        private WaveRewardDataRow GetRewardData(string rewardTableId)
        {
            var table = ExcelDataLoader.GetTable<WaveRewardData>();
            return table?.rows.FirstOrDefault(r => r.RewardTableId == rewardTableId);
        }

        private ResistProfileDataRow GetResistProfile(int id)
        {
            var table = ExcelDataLoader.GetTable<ResistProfileData>();
            return table?.GetById(id);
        }

        private AbilityConfig GetAbilityConfig(int abilityId)
        {
            return CombatDataRegistry.GetAbilityConfig(abilityId);
        }

        private List<int> ParseSkillIds(string skillString)
        {
            var ids = new List<int>();
            if (string.IsNullOrEmpty(skillString)) return ids;

            // Support both semicolon and comma separated values
            var separators = new char[] { ';', ',' };
            var parts = skillString.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                if (int.TryParse(p.Trim(), out var id))
                    ids.Add(id);
            }
            return ids;
        }

        private List<RewardItem> GenerateRewards(string rewardTableId)
        {
            var rewards = new List<RewardItem>();
            var rewardData = GetRewardData(rewardTableId);
            if (rewardData == null) return rewards;

            // Gold
            int gold = Random.Range(rewardData.GoldMin, rewardData.GoldMax + 1);
            if (gold > 0)
            {
                rewards.Add(new RewardItem { Type = RewardType.Gold, Amount = gold });
            }

            // Items (simplified - would use LootTableData in full implementation)
            // For now, just add placeholder items
            if (!string.IsNullOrEmpty(rewardData.ItemPoolId))
            {
                // TODO: Roll from LootTableData
                rewards.Add(new RewardItem { Type = RewardType.Item, ItemId = 1001, Amount = 1 });
            }

            return rewards;
        }

        private void DropRewards(List<RewardItem> rewards)
        {
            Transform dropPoint = GetRewardDropPoint();
            if (dropPoint == null) return;

            foreach (var reward in rewards)
            {
                GameObject prefab = reward.Type == RewardType.Gold ? GoldPrefab : ItemPrefab;
                if (prefab == null) continue;

                Vector3 pos = dropPoint.position + Random.insideUnitSphere * 2f;
                pos.y = dropPoint.position.y;
                GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

                // TODO: Setup reward component on object
            }
        }

        public bool CanStartEarly()
        {
            return _isPreparing && !_earlyStartUsed;
        }

        public void ForceStartWave()
        {
            if (_isPreparing && !_earlyStartUsed)
            {
                _earlyStartUsed = true;
                _prepTimer = 0f;
            }
        }
    }

    public enum RewardType
    {
        Gold,
        Item,
        Material,
        Consumable
    }

    [System.Serializable]
    public class RewardItem
    {
        public RewardType Type;
        public int Amount;
        public int ItemId;
    }
}

