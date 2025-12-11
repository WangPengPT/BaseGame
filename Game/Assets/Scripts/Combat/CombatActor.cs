using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Base combat actor; attach to hero/monster prefabs. Concrete logic to be implemented later.
    /// </summary>
    public class CombatActor : MonoBehaviour
    {
        public string ActorId;
        public StatBlock Stats;
        public IStatusController StatusController;
        public IHitResolver HitResolver;

        public bool IsDead => Stats.Hp <= 0;

        public bool ConsumeMana(float cost)
        {
            if (cost <= 0) return true;
            if (Stats.Mp < cost) return false;
            Stats.Mp -= cost;
            return true;
        }

        public void RestoreMana(float amount)
        {
            Stats.Mp = Mathf.Min(Stats.MaxMp, Stats.Mp + Mathf.Max(0, amount));
        }

        public void RestoreHp(float amount)
        {
            Stats.Hp = Mathf.Min(Stats.MaxHp, Stats.Hp + Mathf.Max(0, amount));
        }
    }
}

