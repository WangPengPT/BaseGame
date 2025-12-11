using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Basic hit resolution: dodge -> crit -> element specials -> resist reduction.
    /// </summary>
    public class HitResolver : MonoBehaviour, IHitResolver
    {
        public bool TryResolveHit(CombatActor attacker, CombatActor target, ref DamagePacket packet)
        {
            if (attacker == null || target == null || target.IsDead) return false;

            // Dodge check
            float dodgeRoll = Random.value;
            if (dodgeRoll < target.Stats.DodgeChance)
            {
                return false;
            }

            return true;
        }

        public void ApplyDamage(CombatActor attacker, CombatActor target, DamagePacket packet)
        {
            if (attacker == null || target == null || target.IsDead) return;

            float dmg = packet.BaseValue;

            // Lightning: random double
            if (packet.AllowLightningDouble && Random.value < 0.25f)
            {
                dmg *= 2f;
            }

            // Fire ignite: apply bonus
            if (packet.Element == ElementType.Fire && packet.IgniteBonusStack > 0)
            {
                dmg *= 1f + packet.IgniteBonusStack;
            }

            // Crit
            float critChance = attacker != null ? attacker.Stats.CritChance + packet.CritChance : packet.CritChance;
            float critMult = attacker != null ? attacker.Stats.CritMultiplier : packet.CritMultiplier;
            if (Random.value < critChance)
            {
                dmg *= critMult;
            }

            // Resist
            dmg *= 1f - GetResist(target, packet);

            // Clamp and apply
            dmg = Mathf.Max(0, dmg);
            target.Stats.Hp = Mathf.Max(0, target.Stats.Hp - dmg);

            // Elemental side-effects
            var status = target.StatusController as StatusController;
            if (status != null)
            {
                if (packet.Element == ElementType.Ice && packet.SlowAmount > 0)
                {
                    status.ApplySlow(packet.SlowAmount, 3f, true);
                }
                if (packet.Element == ElementType.Fire && packet.IgniteBonusStack > 0)
                {
                    status.ApplyIgnite(packet.IgniteBonusStack, 6f);
                }
                if (packet.Element == ElementType.Poison)
                {
                    status.ApplyCorrode(0.08f, 6f);
                }
                if (packet.Element == ElementType.Lightning)
                {
                    status.ApplyChainMark(3f);
                }
            }
        }

        public void ApplyKnockback(CombatActor target, Vector3 direction, float force)
        {
            if (target == null || force <= 0) return;
            direction.y = 0;
            direction.Normalize();
            var rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction * force, ForceMode.VelocityChange);
            }
            else
            {
                target.transform.position += direction * force;
            }
        }

        private float GetResist(CombatActor target, DamagePacket packet)
        {
            var resist = target.Stats.Resist;
            float elementResist = 0f;
            switch (packet.Element)
            {
                case ElementType.Ice: elementResist = resist.IceResist; break;
                case ElementType.Fire: elementResist = resist.FireResist; break;
                case ElementType.Lightning: elementResist = resist.LightningResist; break;
                case ElementType.Poison: elementResist = resist.PoisonResist; break;
                default: elementResist = 0f; break;
            }

            float typeResist = packet.DamageType == DamageType.Physical ? resist.PhysicalResist : resist.MagicalResist;
            float total = Mathf.Clamp01(elementResist + typeResist);
            return total;
        }
    }
}

