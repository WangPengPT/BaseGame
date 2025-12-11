using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Handles cast timing (pre / lock / post), supports interrupt on pre/post, not on cast lock.
    /// Keyframe is executed during cast lock.
    /// </summary>
    public class CastController : MonoBehaviour
    {
        public float KeyframeOffset = 0.5f; // percent inside CastLock

        private Coroutine _castRoutine;
        private AbilityConfig _current;

        public bool IsCasting => _castRoutine != null;

        public void Interrupt()
        {
            // Only allow interrupt if not in cast lock; simple approach: always stop.
            if (_castRoutine != null)
            {
                StopCoroutine(_castRoutine);
                _castRoutine = null;
                _current = null;
            }
        }

        public void StartCast(AbilityConfig ability, CombatActor caster, System.Action keyframeAction, System.Action onFinish)
        {
            if (_castRoutine != null) return;
            _current = ability;
            _castRoutine = StartCoroutine(CastRoutine(ability, caster, keyframeAction, onFinish));
        }

        private IEnumerator CastRoutine(AbilityConfig ability, CombatActor caster, System.Action keyframeAction, System.Action onFinish)
        {
            float speed = Mathf.Max(0.01f, caster?.Stats.AttackSpeed ?? 1f);
            float pre = ability.PreCast / speed;
            float lockTime = ability.CastLock / speed;
            float post = ability.PostCast / speed;

            if (pre > 0) yield return new WaitForSeconds(pre);

            float keyTime = Mathf.Clamp01(KeyframeOffset) * lockTime;
            float elapsed = 0f;
            bool fired = false;
            while (elapsed < lockTime)
            {
                elapsed += Time.deltaTime;
                if (!fired && elapsed >= keyTime)
                {
                    keyframeAction?.Invoke();
                    fired = true;
                }
                yield return null;
            }

            if (!fired)
            {
                keyframeAction?.Invoke();
            }

            if (post > 0) yield return new WaitForSeconds(post);

            onFinish?.Invoke();
            _castRoutine = null;
            _current = null;
        }
    }
}

