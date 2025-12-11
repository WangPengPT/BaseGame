using System.Collections.Generic;
using System.Linq;
using ExcelImporter;
using ExcelData;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Manages hero spawning and initialization in combat.
    /// </summary>
    public class HeroManager : MonoBehaviour
    {
        [Header("Hero Settings")]
        public Transform[] HeroSpawnPoints;
        public GameObject HeroPrefab;
        public int[] InitialHeroIds; // Hero IDs to spawn at start

        private List<CombatActor> _activeHeroes = new List<CombatActor>();

        public System.Action<CombatActor> OnHeroSpawned;
        public System.Action<CombatActor> OnHeroDied;

        private void Start()
        {
            CombatDataRegistry.Initialize(lazy: true);
            SpawnInitialHeroes();
        }

        private void SpawnInitialHeroes()
        {
            if (InitialHeroIds == null || InitialHeroIds.Length == 0)
            {
                Debug.LogWarning("No initial hero IDs configured");
                return;
            }

            if (HeroSpawnPoints == null || HeroSpawnPoints.Length == 0)
            {
                Debug.LogWarning("No hero spawn points configured");
                return;
            }

            for (int i = 0; i < InitialHeroIds.Length && i < HeroSpawnPoints.Length; i++)
            {
                SpawnHero(InitialHeroIds[i], HeroSpawnPoints[i]);
            }
        }

        public void SpawnHero(int heroId, Transform spawnPoint = null)
        {
            if (HeroPrefab == null)
            {
                Debug.LogWarning("HeroPrefab not set");
                return;
            }

            var heroData = CombatDataRegistry.GetHero(heroId);
            if (heroData == null)
            {
                Debug.LogWarning($"Hero {heroId} not found");
                return;
            }

            // Use provided spawn point or pick from array
            Transform spawn = spawnPoint;
            if (spawn == null)
            {
                if (HeroSpawnPoints == null || HeroSpawnPoints.Length == 0)
                {
                    Debug.LogWarning("No spawn points available");
                    return;
                }
                spawn = HeroSpawnPoints[Random.Range(0, HeroSpawnPoints.Length)];
            }

            GameObject heroObj = Instantiate(HeroPrefab, spawn.position, spawn.rotation);

            // Setup hero actor
            var actor = heroObj.GetComponent<CombatActor>();
            if (actor != null)
            {
                // Set base stats from hero data
                actor.Stats.MaxHp = heroData.BaseHp;
                actor.Stats.Hp = actor.Stats.MaxHp;
                actor.Stats.MaxMp = heroData.BaseMp;
                actor.Stats.Mp = actor.Stats.MaxMp;
                actor.Stats.Armor = heroData.Armor;
                actor.Stats.CritChance = heroData.CritChance;
                actor.Stats.CritMultiplier = heroData.CritMultiplier;
                actor.Stats.DodgeChance = heroData.Dodge;
                actor.Stats.AttackSpeed = heroData.AttackSpeed;

                // Apply resistances
                if (heroData.ResistProfileId > 0)
                {
                    var resistProfile = GetResistProfile(heroData.ResistProfileId);
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
                    // Apply direct resistances from hero data if no profile
                    actor.Stats.Resist.PhysicalResist = heroData.PhysicalResist;
                    actor.Stats.Resist.MagicalResist = heroData.MagicalResist;
                    actor.Stats.Resist.IceResist = heroData.IceResist;
                    actor.Stats.Resist.FireResist = heroData.FireResist;
                    actor.Stats.Resist.LightningResist = heroData.LightningResist;
                    actor.Stats.Resist.PoisonResist = heroData.PoisonResist;
                }

                _activeHeroes.Add(actor);
                OnHeroSpawned?.Invoke(actor);
            }

            // Setup AI if available (heroes can have AI too)
            var ai = heroObj.GetComponent<AI.CombatAIController>();
            if (ai != null && !string.IsNullOrEmpty(heroData.BaseSkills))
            {
                // Load abilities from CSV
                var abilityIds = ParseSkillIds(heroData.BaseSkills);
                ai.Abilities.Clear();
                foreach (var abId in abilityIds)
                {
                    var config = CombatDataRegistry.GetAbilityConfig(abId);
                    if (config != null) ai.Abilities.Add(config);
                }
            }
        }

        private ResistProfileDataRow GetResistProfile(int id)
        {
            var table = ExcelDataLoader.GetTable<ResistProfileData>();
            return table?.GetById(id);
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

        public List<CombatActor> GetActiveHeroes()
        {
            // Clean up dead heroes
            _activeHeroes.RemoveAll(h => h == null || h.IsDead);
            return new List<CombatActor>(_activeHeroes);
        }

        public bool HasAliveHeroes()
        {
            return GetActiveHeroes().Count > 0;
        }

        private void Update()
        {
            // Check for hero deaths
            foreach (var hero in _activeHeroes.ToList())
            {
                if (hero != null && hero.IsDead)
                {
                    OnHeroDied?.Invoke(hero);
                }
            }
            _activeHeroes.RemoveAll(h => h == null || h.IsDead);
        }
    }
}

