using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public interface IStatusController
    {
        void ApplySlow(float amount, float duration, bool stack);
        void ApplyIgnite(float stackValue, float duration);
        void ApplyCorrode(float shredPct, float duration);
        void ApplyChainMark(float duration);
    }

    public interface IHitResolver
    {
        bool TryResolveHit(CombatActor attacker, CombatActor target, ref DamagePacket packet);
        void ApplyDamage(CombatActor attacker, CombatActor target, DamagePacket packet);
        void ApplyKnockback(CombatActor target, Vector3 direction, float force);
    }

    public interface ITargetSelector
    {
        List<CombatActor> SelectTargets(CombatActor caster, AbilityShape shape, float radius, float angleDeg, int maxCount);
    }

    public interface IAbility
    {
        AbilityShape Shape { get; }
        float PreCast { get; }
        float CastLock { get; }
        float PostCast { get; }
        float Cooldown { get; }
        float ManaCost { get; }

        /// <summary>
        /// 关键帧回调：由动画事件/定时器驱动，在指定帧触发伤害或效果。
        /// </summary>
        void OnKeyframe(CombatActor caster, AbilityContext ctx);
    }
}

