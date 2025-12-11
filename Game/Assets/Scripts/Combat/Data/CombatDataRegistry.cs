using System;
using System.Collections.Generic;
using ExcelImporter;
using UnityEngine;

namespace Game.Combat.Data
{
    /// <summary>
    /// Helper to fetch ExcelImporter tables and map to runtime configs.
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

        public static AbilityConfig ToConfig(AbilityDataRow row)
        {
            if (row == null) return null;
            return new AbilityConfig
            {
                Id = row.Id,
                Shape = Parse<AbilityShape>(row.Shape, AbilityShape.TargetActor),
                DamageType = Parse<DamageType>(row.Damage_Type, DamageType.Magical),
                Element = Parse<ElementType>(row.Element, ElementType.None),
                BaseValue = row.Base_Value,
                CritBonus = row.Crit_Bonus,
                ManaCost = row.Mana_Cost,
                Cooldown = row.Cooldown,
                PreCast = row.Pre_Cast,
                CastLock = row.Cast_Lock,
                PostCast = row.Post_Cast,
                Radius = row.Radius,
                Angle = row.Angle,
                MaxTargets = row.Max_Targets,
                ChainCount = row.Chain_Count,
                KnockbackForce = row.Knockback_Force,
                IgniteBonus = row.Ignite_Bonus,
                SlowAmount = row.Slow_Amount
            };
        }

        public static List<AbilityConfig> GetAbilityConfigs()
        {
            Initialize();
            var table = ExcelDataLoader.GetTable<AbilityData>();
            var list = new List<AbilityConfig>();
            if (table == null) return list;
            foreach (var row in table.rows)
            {
                var cfg = ToConfig(row);
                if (cfg != null) list.Add(cfg);
            }
            return list;
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

