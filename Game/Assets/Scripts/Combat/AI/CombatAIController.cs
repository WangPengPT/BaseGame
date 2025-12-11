using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Combat.AI
{
    /// <summary>
    /// Simple AI: pick nearest target, choose first ready ability, run cast timings with keyframe execution.
    /// </summary>
    public class CombatAIController : MonoBehaviour
    {
        public CombatActor Actor;
        public List<AbilityConfig> Abilities = new List<AbilityConfig>();
        public float ScanRadius = 8f;
        public LayerMask TargetMask = ~0;
        public float KeyframeOffset = 0.0f; // keyframe timing offset within CastLock

        private AbilityExecutor _executor;
        private CastController _castController;
        private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();

        private void Awake()
        {
            Actor ??= GetComponent<CombatActor>();
            _executor ??= GetComponent<AbilityExecutor>();
            _castController ??= GetComponent<CastController>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            DecayCooldowns(dt);

            if (Actor == null || Actor.IsDead) return;
            if (_castController != null && _castController.IsCasting) return;

            var target = FindNearestTarget();
            if (target == null) return;

            var ability = PickReadyAbility();
            if (ability == null) return;

            if (!Actor.ConsumeMana(ability.ManaCost)) return;

            if (_castController != null)
            {
                _castController.KeyframeOffset = KeyframeOffset;
                _castController.StartCast(ability, Actor,
                    () => ExecuteAtKeyframe(ability, target),
                    () => SetCooldown(ability));
            }
        }

        private void ExecuteAtKeyframe(AbilityConfig ability, CombatActor target)
        {
            if (_executor == null || ability == null || Actor == null) return;
            Vector3 dir = (target.transform.position - Actor.transform.position).normalized;
            _executor.Execute(ability, Actor, target.transform.position, dir);
        }

        private CombatActor FindNearestTarget()
        {
            Collider[] hits = Physics.OverlapSphere(Actor.transform.position, ScanRadius, TargetMask);
            float bestDist = float.MaxValue;
            CombatActor best = null;
            foreach (var h in hits)
            {
                var act = h.GetComponentInParent<CombatActor>();
                if (act == null || act == Actor || act.IsDead) continue;
                float d = Vector3.SqrMagnitude(act.transform.position - Actor.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = act;
                }
            }
            return best;
        }

        private AbilityConfig PickReadyAbility()
        {
            foreach (var ab in Abilities)
            {
                if (ab == null) continue;
                if (IsOnCooldown(ab.Id)) continue;
                if (ab.ManaCost > Actor.Stats.Mp) continue;
                return ab;
            }
            return null;
        }

        private void DecayCooldowns(float dt)
        {
            var keys = _cooldowns.Keys.ToList();
            foreach (var k in keys)
            {
                _cooldowns[k] = Mathf.Max(0, _cooldowns[k] - dt);
            }
        }

        private bool IsOnCooldown(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return _cooldowns.TryGetValue(id, out var t) && t > 0;
        }

        private void SetCooldown(AbilityConfig ab)
        {
            if (string.IsNullOrEmpty(ab.Id)) return;
            _cooldowns[ab.Id] = ab.Cooldown;
        }
    }
}

