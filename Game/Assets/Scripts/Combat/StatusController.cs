using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Lightweight status controller to track slow/ignite/corrode/chain marks.
    /// This is minimal and can be replaced with your full status system.
    /// </summary>
    public class StatusController : MonoBehaviour, IStatusController
    {
        public float SlowStacks { get; private set; }
        public float SlowExpireTimer { get; private set; }
        public float IgniteStacks { get; private set; }
        public float IgniteExpireTimer { get; private set; }
        public float CorrodeTimer { get; private set; }
        public float CorrodeValue { get; private set; }
        public float ChainMarkTimer { get; private set; }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (SlowExpireTimer > 0) SlowExpireTimer -= dt; else SlowStacks = 0;
            if (IgniteExpireTimer > 0) IgniteExpireTimer -= dt; else IgniteStacks = 0;
            if (CorrodeTimer > 0) CorrodeTimer -= dt; else CorrodeValue = 0;
            if (ChainMarkTimer > 0) ChainMarkTimer -= dt;
        }

        public void ApplySlow(float amount, float duration, bool stack)
        {
            if (stack) SlowStacks += amount;
            else SlowStacks = Mathf.Max(SlowStacks, amount);
            SlowExpireTimer = Mathf.Max(SlowExpireTimer, duration);
        }

        public void ApplyIgnite(float stackValue, float duration)
        {
            IgniteStacks += stackValue;
            IgniteExpireTimer = Mathf.Max(IgniteExpireTimer, duration);
        }

        public void ApplyCorrode(float shredPct, float duration)
        {
            CorrodeValue = Mathf.Max(CorrodeValue, shredPct);
            CorrodeTimer = Mathf.Max(CorrodeTimer, duration);
        }

        public void ApplyChainMark(float duration)
        {
            ChainMarkTimer = Mathf.Max(ChainMarkTimer, duration);
        }
    }
}

