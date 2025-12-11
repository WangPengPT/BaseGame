using System;
using UnityEngine;

namespace Game.Combat
{
    [Serializable]
    public class AbilityConfig
    {
        public string Id;
        public AbilityShape Shape;
        public DamageType DamageType;
        public ElementType Element;
        public float BaseValue;
        public float CritBonus;
        public float CritMultiplier = 2f;
        public float ManaCost;
        public float Cooldown;
        public float PreCast;
        public float CastLock;
        public float PostCast;
        public float Radius = 3f;
        public float Angle = 90f;
        public int MaxTargets = 1;
        public int ChainCount = 0;
        public float KnockbackForce = 0;
        public float IgniteBonus = 0;
        public float SlowAmount = 0;

        public DamagePacket ToPacket()
        {
            return new DamagePacket
            {
                DamageType = DamageType,
                Element = Element,
                BaseValue = BaseValue,
                CritChance = CritBonus,
                CritMultiplier = CritMultiplier,
                IgniteBonusStack = IgniteBonus,
                SlowAmount = SlowAmount,
                AllowLightningDouble = Element == ElementType.Lightning,
                ChainCount = ChainCount,
                KnockbackForce = KnockbackForce
            };
        }
    }
}

