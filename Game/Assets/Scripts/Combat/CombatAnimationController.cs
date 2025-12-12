using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 处理战斗单位的动画播放，包括移动、攻击、受伤、死亡等动画
    /// </summary>
    [RequireComponent(typeof(CombatActor))]
    [RequireComponent(typeof(Animator))]
    public class CombatAnimationController : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [SerializeField] private string moveParameter = "IsMoving";
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private string attackParameter = "Attack";
        [SerializeField] private string hitParameter = "Hit";
        [SerializeField] private string deathParameter = "Death";
        [SerializeField] private string attackSpeedParameter = "AttackSpeed";

        private CombatActor _actor;
        private Animator _animator;
        private CombatMovementController _movementController;
        private float _lastHp;

        // 动画参数哈希（性能优化）
        private int _hashMove;
        private int _hashSpeed;
        private int _hashAttack;
        private int _hashHit;
        private int _hashDeath;
        private int _hashAttackSpeed;

        private void Awake()
        {
            _actor = GetComponent<CombatActor>();
            _animator = GetComponent<Animator>();
            _movementController = GetComponent<CombatMovementController>();

            // 缓存动画参数哈希
            _hashMove = Animator.StringToHash(moveParameter);
            _hashSpeed = Animator.StringToHash(speedParameter);
            _hashAttack = Animator.StringToHash(attackParameter);
            _hashHit = Animator.StringToHash(hitParameter);
            _hashDeath = Animator.StringToHash(deathParameter);
            _hashAttackSpeed = Animator.StringToHash(attackSpeedParameter);

            _lastHp = _actor != null ? _actor.Stats.Hp : 0;
        }

        private void Update()
        {
            if (_actor == null || _animator == null) return;

            UpdateMovementAnimation();
            UpdateHealthAnimation();
            UpdateAttackSpeed();
        }

        /// <summary>
        /// 更新移动动画
        /// </summary>
        private void UpdateMovementAnimation()
        {
            if (_movementController == null) return;

            bool isMoving = _movementController.IsMoving;
            float speed = _movementController.Velocity.magnitude;

            // 设置移动状态
            if (_animator.parameters.Length > 0)
            {
                // 检查参数是否存在，避免运行时错误
                if (HasParameter(_hashMove))
                {
                    _animator.SetBool(_hashMove, isMoving);
                }

                if (HasParameter(_hashSpeed))
                {
                    _animator.SetFloat(_hashSpeed, speed);
                }
            }
        }

        /// <summary>
        /// 更新生命值相关动画（受伤、死亡）
        /// </summary>
        private void UpdateHealthAnimation()
        {
            if (_actor.IsDead)
            {
                if (HasParameter(_hashDeath))
                {
                    _animator.SetTrigger(_hashDeath);
                }
                return;
            }

            // 检测是否受伤
            float currentHp = _actor.Stats.Hp;
            if (currentHp < _lastHp && _lastHp > 0)
            {
                // 受到伤害，播放受伤动画
                if (HasParameter(_hashHit))
                {
                    _animator.SetTrigger(_hashHit);
                }
            }
            _lastHp = currentHp;
        }

        /// <summary>
        /// 更新攻击速度
        /// </summary>
        private void UpdateAttackSpeed()
        {
            if (_actor != null && HasParameter(_hashAttackSpeed))
            {
                float attackSpeed = _actor.Stats.AttackSpeed;
                _animator.SetFloat(_hashAttackSpeed, attackSpeed);
            }
        }

        /// <summary>
        /// 播放攻击动画
        /// </summary>
        public void PlayAttackAnimation()
        {
            if (_animator != null && HasParameter(_hashAttack))
            {
                _animator.SetTrigger(_hashAttack);
            }
        }

        /// <summary>
        /// 播放受伤动画
        /// </summary>
        public void PlayHitAnimation()
        {
            if (_animator != null && HasParameter(_hashHit))
            {
                _animator.SetTrigger(_hashHit);
            }
        }

        /// <summary>
        /// 播放死亡动画
        /// </summary>
        public void PlayDeathAnimation()
        {
            if (_animator != null && HasParameter(_hashDeath))
            {
                _animator.SetTrigger(_hashDeath);
            }
        }

        /// <summary>
        /// 检查动画参数是否存在
        /// </summary>
        private bool HasParameter(int hash)
        {
            if (_animator == null) return false;

            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                if (param.nameHash == hash)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取动画器
        /// </summary>
        public Animator GetAnimator()
        {
            return _animator;
        }
    }
}

