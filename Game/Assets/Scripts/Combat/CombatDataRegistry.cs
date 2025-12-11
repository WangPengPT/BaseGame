using System;
using System.Collections.Generic;
using System.Linq;
using ExcelImporter;
using ExcelData;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Helper to fetch ExcelData tables and map to runtime configs.
    /// </summary>
    public static class CombatDataRegistry
    {
        private static bool _initialized;

        public static void Initialize(bool lazy = true)
        {
            if (_initialized) return;
            if (lazy) ExcelDataLoader.InitializeLazy();
            else ExcelDataLoader.Initialize();
            _initialized = true;
        }

        public static AbilityConfig ToConfig(SkillDataRow row)
        {
            if (row == null) return null;
            return new AbilityConfig
            {
                Id = row.Id.ToString(),
                Shape = Parse<AbilityShape>(row.Shape, AbilityShape.TargetActor),
                DamageType = Parse<DamageType>(row.DamageType, DamageType.Magical),
                Element = Parse<ElementType>(row.Element, ElementType.None),
                BaseValue = row.BaseValue,
                CritBonus = row.CritBonus,
                ManaCost = row.ManaCost,
                Cooldown = row.Cooldown,
                PreCast = row.PreCast,
                CastLock = row.CastLock,
                PostCast = row.PostCast,
                Radius = row.Radius,
                Angle = row.Angle,
                MaxTargets = 1, // Not in SkillData, using default
                ChainCount = row.ChainCount,
                KnockbackForce = row.KnockbackForce,
                IgniteBonus = row.IgniteBonus,
                SlowAmount = row.SlowAmount
            };
        }

        public static List<AbilityConfig> GetAbilityConfigs()
        {
            Initialize();
            var table = ExcelDataLoader.GetTable<SkillData>();
            var list = new List<AbilityConfig>();
            if (table == null) return list;
            foreach (var row in table.rows)
            {
                var cfg = ToConfig(row);
                if (cfg != null) list.Add(cfg);
            }
            return list;
        }

        public static AbilityConfig GetAbilityConfig(int abilityId)
        {
            Initialize();
            var table = ExcelDataLoader.GetTable<SkillData>();
            var row = table?.GetById(abilityId);
            return ToConfig(row);
        }

        public static HeroDataRow GetHero(int id)
        {
            Initialize();
            var t = ExcelDataLoader.GetTable<HeroData>();
            return t != null ? t.GetById(id) : null;
        }

        public static EnemyDataRow GetEnemy(int id)
        {
            Initialize();
            var t = ExcelDataLoader.GetTable<EnemyData>();
            return t != null ? t.GetById(id) : null;
        }

        private static TEnum Parse<TEnum>(string value, TEnum fallback) where TEnum : struct
        {
            if (Enum.TryParse<TEnum>(value, true, out var r)) return r;
            return fallback;
        }
    }
}

