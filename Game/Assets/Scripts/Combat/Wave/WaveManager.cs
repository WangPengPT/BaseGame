using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Combat;
using ExcelImporter;
using ExcelData;
using UnityEngine;

namespace Game.Combat.Wave
{
    /// <summary>
    /// Manages wave-based combat: spawns enemies per wave, detects completion, drops rewards.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Settings")]
        public int CurrentWaveIndex = 0;
        public float PrepTime = 5f;
        public bool AutoStartNextWave = true;
        public float EarlyStartBonus = 0.1f; // 10% bonus for early start

        [Header("Spawn Settings")]
        public Transform[] SpawnPoints;
        public GameObject EnemyPrefab;
        public LayerMask EnemyLayer;

        [Header("Reward Settings")]
        public Transform RewardDropPoint;
        public GameObject GoldPrefab;
        public GameObject ItemPrefab;

        private List<CombatActor> _activeEnemies = new List<CombatActor>();
        private bool _isPreparing = false;
        private bool _isWaveActive = false;
        private float _prepTimer = 0f;
        private float _waveStartTime = 0f;
        private bool _earlyStartUsed = false;

        public System.Action<int> OnWaveStarted;
        public System.Action<int> OnWaveCompleted;
        public System.Action<float> OnPrepTimerChanged;
        public System.Action<List<RewardItem>> OnRewardsDropped;

        private void Start()
        {
            CombatDataRegistry.Initialize(lazy: true);
            StartCoroutine(WaveLoop());
        }

        private IEnumerator WaveLoop()
        {
            while (true)
            {
                // Prep phase
                yield return StartCoroutine(PrepPhase());

                // Wave phase
                yield return StartCoroutine(WavePhase());

                // Reward phase
                yield return StartCoroutine(RewardPhase());
            }
        }

        private IEnumerator PrepPhase()
        {
            _isPreparing = true;
            _prepTimer = PrepTime;
            _earlyStartUsed = false;

            OnWaveStarted?.Invoke(CurrentWaveIndex);

            while (_prepTimer > 0f)
            {
                _prepTimer -= Time.deltaTime;
                OnPrepTimerChanged?.Invoke(_prepTimer);

                // Check for early start (player can press a key or button)
                if (Input.GetKeyDown(KeyCode.Space) && !_earlyStartUsed)
                {
                    _earlyStartUsed = true;
                    break; // Start wave early
                }

                yield return null;
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
                yield break;
            }

            // Apply difficulty scalar
            float difficulty = waveData.DifficultyScalar;

            // Spawn enemies
            var entries = GetWaveEntries(waveData.Id);
            foreach (var entry in entries)
            {
                yield return new WaitForSeconds(entry.SpawnDelay);

                for (int i = 0; i < entry.Count; i++)
                {
                    SpawnEnemy(entry.EnemyId, entry.IsElite, difficulty);
                    yield return new WaitForSeconds(0.2f); // Small delay between spawns
                }
            }

            // Wait for all enemies to be defeated
            while (_activeEnemies.Count > 0)
            {
                _activeEnemies.RemoveAll(e => e == null || e.IsDead);
                yield return new WaitForSeconds(0.5f);
            }

            _isWaveActive = false;
            OnWaveCompleted?.Invoke(CurrentWaveIndex);
        }

        private IEnumerator RewardPhase()
        {
            var waveData = GetWaveData(CurrentWaveIndex);
            if (waveData == null) yield break;

            var rewards = GenerateRewards(waveData.RewardTableId, _earlyStartUsed);
            DropRewards(rewards);

            OnRewardsDropped?.Invoke(rewards);

            yield return new WaitForSeconds(2f); // Brief pause before next wave

            CurrentWaveIndex++;
        }

        private void SpawnEnemy(int enemyId, bool isElite, float difficultyScalar)
        {
            if (EnemyPrefab == null || SpawnPoints == null || SpawnPoints.Length == 0)
            {
                Debug.LogWarning("EnemyPrefab or SpawnPoints not set");
                return;
            }

            var enemyData = CombatDataRegistry.GetEnemy(enemyId);
            if (enemyData == null)
            {
                Debug.LogWarning($"Enemy {enemyId} not found");
                return;
            }

            // Pick random spawn point
            Transform spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
            GameObject enemyObj = Instantiate(EnemyPrefab, spawnPoint.position, spawnPoint.rotation);

            // Setup enemy actor
            var actor = enemyObj.GetComponent<CombatActor>();
            if (actor != null)
            {
                // Apply difficulty scaling
                actor.Stats.MaxHp = enemyData.BaseHp * difficultyScalar;
                actor.Stats.Hp = actor.Stats.MaxHp;
                actor.Stats.MaxMp = enemyData.BaseMp;
                actor.Stats.Mp = actor.Stats.MaxMp;
                actor.Stats.Armor = enemyData.Armor ? 1f : 0f; // Convert bool to float

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
            return table?.rows.Where(r => r.WaveId == waveId).ToList() ?? new List<WaveEntryDataRow>();
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

        private List<RewardItem> GenerateRewards(string rewardTableId, bool earlyStart)
        {
            var rewards = new List<RewardItem>();
            var rewardData = GetRewardData(rewardTableId);
            if (rewardData == null) return rewards;

            float bonus = earlyStart ? EarlyStartBonus : 0f;

            // Gold
            int gold = Random.Range(rewardData.GoldMin, rewardData.GoldMax + 1);
            gold = Mathf.RoundToInt(gold * (1f + bonus));
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
            if (RewardDropPoint == null) return;

            foreach (var reward in rewards)
            {
                GameObject prefab = reward.Type == RewardType.Gold ? GoldPrefab : ItemPrefab;
                if (prefab == null) continue;

                Vector3 pos = RewardDropPoint.position + Random.insideUnitSphere * 2f;
                pos.y = RewardDropPoint.position.y;
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

