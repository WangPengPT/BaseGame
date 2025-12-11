using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Executes ability effects at keyframes.
    /// </summary>
    public class AbilityExecutor : MonoBehaviour
    {
        public ITargetSelector Selector;
        public IHitResolver Resolver;

        private void Awake()
        {
            Selector ??= GetComponent<ITargetSelector>();
            Resolver ??= GetComponent<IHitResolver>();
        }

        public void Execute(AbilityConfig config, CombatActor caster, Vector3 aimPoint, Vector3 aimDir)
        {
            if (config == null || caster == null) return;
            if (Resolver == null) Resolver = caster.GetComponent<IHitResolver>();
            if (Selector == null) Selector = caster.GetComponent<ITargetSelector>();
            if (Resolver == null || Selector == null) return;

            var packet = config.ToPacket();

            switch (config.Shape)
            {
                case AbilityShape.Cone:
                    HandleCone(config, caster, packet);
                    break;
                case AbilityShape.TargetActor:
                    HandleTargetActor(config, caster, packet);
                    break;
                case AbilityShape.Projectile:
                    HandleProjectileInstant(config, caster, packet, aimDir);
                    break;
                case AbilityShape.Chain:
                    HandleChain(config, caster, packet);
                    break;
                case AbilityShape.SelfAoe:
                    HandleSelfAoe(config, caster, packet);
                    break;
                default:
                    HandleTargetActor(config, caster, packet);
                    break;
            }
        }

        private void HandleCone(AbilityConfig cfg, CombatActor caster, DamagePacket packet)
        {
            var targets = Selector.SelectTargets(caster, AbilityShape.Cone, cfg.Radius, cfg.Angle, cfg.MaxTargets);
            foreach (var t in targets)
            {
                ApplyHit(caster, t, packet);
            }
        }

        private void HandleSelfAoe(AbilityConfig cfg, CombatActor caster, DamagePacket packet)
        {
            var targets = Selector.SelectTargets(caster, AbilityShape.SelfAoe, cfg.Radius, 360f, cfg.MaxTargets);
            foreach (var t in targets)
            {
                ApplyHit(caster, t, packet);
            }
        }

        private void HandleTargetActor(AbilityConfig cfg, CombatActor caster, DamagePacket packet)
        {
            var targets = Selector.SelectTargets(caster, AbilityShape.TargetActor, cfg.Radius, 360f, 1);
            if (targets.Count > 0)
            {
                ApplyHit(caster, targets[0], packet);
            }
        }

        private void HandleProjectileInstant(AbilityConfig cfg, CombatActor caster, DamagePacket packet, Vector3 dir)
        {
            // Simplified: raycast as instant projectile; you can replace with real projectile prefab.
            if (dir == Vector3.zero) dir = caster.transform.forward;
            if (Physics.Raycast(caster.transform.position + Vector3.up * 0.5f, dir, out var hit, cfg.Radius))
            {
                var target = hit.collider.GetComponentInParent<CombatActor>();
                if (target != null) ApplyHit(caster, target, packet);
            }
        }

        private void HandleChain(AbilityConfig cfg, CombatActor caster, DamagePacket packet)
        {
            var targets = Selector.SelectTargets(caster, AbilityShape.TargetActor, cfg.Radius, 360f, 1);
            if (targets.Count == 0) return;
            var first = targets[0];
            ApplyHit(caster, first, packet);

            var remaining = new List<CombatActor>(Selector.SelectTargets(caster, AbilityShape.SelfAoe, cfg.Radius, 360f, cfg.ChainCount + 1));
            remaining.Remove(first);
            int jumps = cfg.ChainCount;
            var current = first;
            while (jumps-- > 0 && remaining.Count > 0)
            {
                var next = remaining[0];
                remaining.RemoveAt(0);
                ApplyHit(caster, next, packet);
                current = next;
            }
        }

        private void ApplyHit(CombatActor caster, CombatActor target, DamagePacket packet)
        {
            if (Resolver.TryResolveHit(caster, target, ref packet))
            {
                Resolver.ApplyDamage(caster, target, packet);
                if (packet.KnockbackForce > 0)
                {
                    var dir = (target.transform.position - caster.transform.position).normalized;
                    Resolver.ApplyKnockback(target, dir, packet.KnockbackForce);
                }
            }
        }
    }
}

