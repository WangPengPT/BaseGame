using System;

namespace Game.Combat
{
    public enum DamageType
    {
        Physical = 0,
        Magical = 1
    }

    public enum ElementType
    {
        None = 0,
        Ice = 1,
        Fire = 2,
        Lightning = 3,
        Poison = 4
    }

    /// <summary>
    /// Ability spatial shape for targeting.
    /// </summary>
    public enum AbilityShape
    {
        Cone,
        Projectile,
        Chain,
        TargetPoint,
        TargetActor,
        SelfAoe
    }

    /// <summary>
    /// Timing phase of an ability cast.
    /// </summary>
    public enum CastPhase
    {
        None,
        PreCast,   // 前摇，可打断
        CastLock,  // 硬直，不可打断
        PostCast   // 后摇，可打断
    }
}

