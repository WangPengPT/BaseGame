using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Placeholder projectile launcher. Replace SpawnProjectile with your prefab/FX.
    /// </summary>
    public class ProjectileLauncher : MonoBehaviour
    {
        public float DefaultSpeed = 12f;
        public LayerMask HitMask = ~0;

        public void Launch(AbilityConfig ability, CombatActor caster, Vector3 direction)
        {
            if (ability == null || caster == null) return;
            if (direction == Vector3.zero) direction = caster.transform.forward;
            // Simplified ray approach; replace with real projectile spawn:
            if (Physics.Raycast(caster.transform.position + Vector3.up * 0.5f, direction, out var hit, ability.Radius, HitMask))
            {
                var target = hit.collider.GetComponentInParent<CombatActor>();
                if (target == null) return;
                var resolver = caster.GetComponent<IHitResolver>();
                if (resolver == null) return;
                var packet = ability.ToPacket();
                if (resolver.TryResolveHit(caster, target, ref packet))
                {
                    resolver.ApplyDamage(caster, target, packet);
                    if (packet.KnockbackForce > 0)
                    {
                        resolver.ApplyKnockback(target, direction, packet.KnockbackForce);
                    }
                }
            }
        }
    }
}

