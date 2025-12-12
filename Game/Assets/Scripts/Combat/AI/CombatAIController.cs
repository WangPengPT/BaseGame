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
        [Header("Movement")]
        public float attackRange = 2f; // 攻击范围
        public float followDistance = 5f; // 跟随距离

        private AbilityExecutor _executor;
        private CastController _castController;
        private CombatMovementController _movementController;
        private CombatAnimationController _animationController;
        private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();
        private CombatActor _currentTarget;

        private void Awake()
        {
            Actor ??= GetComponent<CombatActor>();
            _executor ??= GetComponent<AbilityExecutor>();
            _castController ??= GetComponent<CastController>();
            _movementController ??= GetComponent<CombatMovementController>();
            _animationController ??= GetComponent<CombatAnimationController>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            DecayCooldowns(dt);

            if (Actor == null || Actor.IsDead)
            {
                if (_movementController != null)
                    _movementController.Stop();
                return;
            }

            if (_castController != null && _castController.IsCasting)
            {
                // 施法时停止移动
                if (_movementController != null)
                    _movementController.Stop();
                return;
            }

            var target = FindNearestTarget();
            _currentTarget = target;

            if (target == null)
            {
                // 没有目标时停止移动
                if (_movementController != null)
                    _movementController.Stop();
                return;
            }

            // 处理移动：移动到攻击范围
            HandleMovement(target);

            // 检查是否在攻击范围内
            float distanceToTarget = Vector3.Distance(Actor.transform.position, target.transform.position);
            if (distanceToTarget > attackRange)
            {
                // 不在攻击范围，继续移动
                return;
            }

            // 在攻击范围内，停止移动并尝试攻击
            if (_movementController != null)
                _movementController.Stop();

            // 面向目标
            Vector3 direction = (target.transform.position - Actor.transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Actor.transform.rotation = Quaternion.LookRotation(direction);
            }

            var ability = PickReadyAbility();
            if (ability == null) return;

            if (!Actor.ConsumeMana(ability.ManaCost)) return;

            // 播放攻击动画
            if (_animationController != null)
                _animationController.PlayAttackAnimation();

            if (_castController != null)
            {
                _castController.KeyframeOffset = KeyframeOffset;
                _castController.StartCast(ability, Actor,
                    () => ExecuteAtKeyframe(ability, target),
                    () => SetCooldown(ability));
            }
        }

        private void HandleMovement(CombatActor target)
        {
            if (_movementController == null) return;

            float distanceToTarget = Vector3.Distance(Actor.transform.position, target.transform.position);

            if (distanceToTarget > attackRange)
            {
                // 移动到攻击范围
                _movementController.FollowTarget(target.transform, attackRange);
            }
            else
            {
                // 在攻击范围内，停止移动
                _movementController.Stop();
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

