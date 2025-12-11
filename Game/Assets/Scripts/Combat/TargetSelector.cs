using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Combat
{
    public class TargetSelector : MonoBehaviour, ITargetSelector
    {
        public LayerMask ActorMask = ~0;

        public List<CombatActor> SelectTargets(CombatActor caster, AbilityShape shape, float radius, float angleDeg, int maxCount)
        {
            var results = new List<CombatActor>();
            if (caster == null) return results;

            var hits = Physics.OverlapSphere(caster.transform.position, radius, ActorMask);
            foreach (var col in hits)
            {
                var actor = col.GetComponentInParent<CombatActor>();
                if (actor == null || actor == caster || actor.IsDead) continue;

                if (shape == AbilityShape.Cone)
                {
                    Vector3 to = (actor.transform.position - caster.transform.position);
                    to.y = 0;
                    var dir = caster.transform.forward;
                    float angle = Vector3.Angle(dir, to);
                    if (angle > angleDeg * 0.5f) continue;
                }

                results.Add(actor);
                if (maxCount > 0 && results.Count >= maxCount) break;
            }

            return results;
        }
    }
}

