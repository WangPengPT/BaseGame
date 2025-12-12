using UnityEngine;
using UnityEngine.AI;

namespace Game.Combat
{
    /// <summary>
    /// 处理战斗单位的移动逻辑，支持 NavMesh 导航和手动移动
    /// </summary>
    [RequireComponent(typeof(CombatActor))]
    public class CombatMovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float rotationSpeed = 360f;
        [SerializeField] private float stoppingDistance = 0.5f;
        [SerializeField] private bool useNavMesh = true;

        private CombatActor _actor;
        private NavMeshAgent _navAgent;
        private Vector3 _targetPosition;
        private Transform _targetTransform;
        private bool _isMoving = false;
        private bool _isStopped = false;

        // 移动状态
        public bool IsMoving => _isMoving && !_isStopped;
        public Vector3 Velocity { get; private set; }
        public float Speed => moveSpeed;

        private void Awake()
        {
            _actor = GetComponent<CombatActor>();
            
            if (useNavMesh)
            {
                _navAgent = GetComponent<NavMeshAgent>();
                if (_navAgent == null)
                {
                    _navAgent = gameObject.AddComponent<NavMeshAgent>();
                }
                
                _navAgent.speed = moveSpeed;
                _navAgent.angularSpeed = rotationSpeed;
                _navAgent.stoppingDistance = stoppingDistance;
                _navAgent.updateRotation = true;
                _navAgent.updatePosition = true;
            }
        }

        private void Update()
        {
            if (_actor == null || _actor.IsDead)
            {
                Stop();
                return;
            }

            if (_isStopped)
            {
                Velocity = Vector3.zero;
                _isMoving = false;
                return;
            }

            if (useNavMesh && _navAgent != null)
            {
                UpdateNavMeshMovement();
            }
            else
            {
                UpdateManualMovement();
            }
        }

        /// <summary>
        /// 移动到指定位置
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            _targetPosition = position;
            _targetTransform = null;
            _isStopped = false;

            if (useNavMesh && _navAgent != null)
            {
                if (_navAgent.isOnNavMesh)
                {
                    _navAgent.SetDestination(position);
                    _isMoving = true;
                }
            }
            else
            {
                _isMoving = true;
            }
        }

        /// <summary>
        /// 跟随目标移动
        /// </summary>
        public void FollowTarget(Transform target, float distance = 0f)
        {
            if (target == null)
            {
                Stop();
                return;
            }

            _targetTransform = target;
            _isStopped = false;
            _isMoving = true;

            if (useNavMesh && _navAgent != null)
            {
                if (distance > 0)
                {
                    _navAgent.stoppingDistance = distance;
                }
            }
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void Stop()
        {
            _isStopped = true;
            _isMoving = false;
            _targetTransform = null;

            if (useNavMesh && _navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.ResetPath();
            }
        }

        /// <summary>
        /// 恢复移动
        /// </summary>
        public void Resume()
        {
            _isStopped = false;
            
            if (useNavMesh && _navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = false;
            }
        }

        /// <summary>
        /// 设置移动速度
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
            if (_navAgent != null)
            {
                _navAgent.speed = speed;
            }
        }

        private void UpdateNavMeshMovement()
        {
            if (_navAgent == null || !_navAgent.isOnNavMesh) return;

            // 如果有跟随目标，持续更新目标位置
            if (_targetTransform != null)
            {
                if (Vector3.Distance(transform.position, _targetTransform.position) > stoppingDistance)
                {
                    _navAgent.SetDestination(_targetTransform.position);
                    _navAgent.isStopped = false;
                }
                else
                {
                    _navAgent.isStopped = true;
                    _isMoving = false;
                }
            }
            else if (_isMoving)
            {
                // 检查是否到达目标
                if (!_navAgent.pathPending && _navAgent.remainingDistance < stoppingDistance)
                {
                    _navAgent.isStopped = true;
                    _isMoving = false;
                }
                else
                {
                    _navAgent.isStopped = false;
                }
            }

            // 更新速度用于动画
            Velocity = _navAgent.velocity;
            _isMoving = _navAgent.velocity.magnitude > 0.1f;
        }

        private void UpdateManualMovement()
        {
            Vector3 targetPos = _targetTransform != null ? _targetTransform.position : _targetPosition;
            Vector3 direction = (targetPos - transform.position);
            direction.y = 0; // 保持在地面

            float distance = direction.magnitude;

            if (distance > stoppingDistance && _isMoving)
            {
                // 移动
                Vector3 moveDirection = direction.normalized;
                transform.position += moveDirection * moveSpeed * Time.deltaTime;

                // 旋转朝向移动方向
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }

                Velocity = moveDirection * moveSpeed;
            }
            else
            {
                _isMoving = false;
                Velocity = Vector3.zero;
            }
        }

        private void OnDisable()
        {
            Stop();
        }
    }
}

