using System;
using UnityEngine;

namespace Game.Combat
{
    [Serializable]
    public struct ResistanceProfile
    {
        public float PhysicalResist;
        public float MagicalResist;
        public float IceResist;
        public float FireResist;
        public float LightningResist;
        public float PoisonResist;
    }

    [Serializable]
    public struct StatBlock
    {
        public float MaxHp;
        public float Hp;
        public float MaxMp;
        public float Mp;
        public float Armor;
        public float CritChance;
        public float CritMultiplier;
        public float DodgeChance;
        public float AttackSpeed; // 用于缩放前后摇
        public ResistanceProfile Resist;
    }

    public struct DamagePacket
    {
        public DamageType DamageType;
        public ElementType Element;
        public float BaseValue;
        public float CritChance;
        public float CritMultiplier;
        public float IgniteBonusStack;   // 火：叠加增伤或后续 DoT
        public float SlowAmount;         // 冰：附加减速
        public bool AllowLightningDouble;// 电：是否允许随机翻倍
        public int ChainCount;           // 连锁次数
        public float KnockbackForce;     // 击退力度
    }

    public class AbilityContext
    {
        public CombatActor Caster;
        public Vector3 AimPoint;
        public Vector3 AimDirection;
        public CastPhase Phase;
    }
}

