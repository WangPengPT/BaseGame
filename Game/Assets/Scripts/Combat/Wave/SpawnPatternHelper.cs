using UnityEngine;
using System.Collections.Generic;

namespace Game.Combat.Wave
{
    /// <summary>
    /// 辅助类：根据不同的生成模式计算敌人生成位置
    /// </summary>
    public static class SpawnPatternHelper
    {
        /// <summary>
        /// 根据生成模式计算生成位置
        /// </summary>
        /// <param name="pattern">生成模式：circle, arc, center</param>
        /// <param name="spawnPoints">可用的生成点数组</param>
        /// <param name="centerPoint">中心点（用于circle和arc模式）</param>
        /// <param name="count">需要生成的数量</param>
        /// <param name="index">当前生成索引（用于在模式中分布位置）</param>
        /// <returns>生成位置</returns>
        public static Vector3 GetSpawnPosition(string pattern, Transform[] spawnPoints, Transform centerPoint, int count, int index)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("SpawnPoints为空，使用默认位置");
                return Vector3.zero;
            }

            pattern = pattern?.ToLower() ?? "circle";

            switch (pattern)
            {
                case "circle":
                    return GetCircleSpawnPosition(spawnPoints, centerPoint, count, index);
                
                case "arc":
                    return GetArcSpawnPosition(spawnPoints, centerPoint, count, index);
                
                case "center":
                    return GetCenterSpawnPosition(spawnPoints, centerPoint);
                
                default:
                    // 默认随机选择
                    return GetRandomSpawnPosition(spawnPoints);
            }
        }

        /// <summary>
        /// 圆形生成：围绕中心点均匀分布
        /// </summary>
        private static Vector3 GetCircleSpawnPosition(Transform[] spawnPoints, Transform centerPoint, int count, int index)
        {
            if (centerPoint == null)
            {
                // 如果没有中心点，使用第一个spawn point作为中心
                centerPoint = spawnPoints[0];
            }

            Vector3 center = centerPoint.position;
            float radius = 5f; // 默认半径，可以根据需要调整

            // 计算角度（360度均匀分布）
            float angleStep = 360f / count;
            float angle = angleStep * index * Mathf.Deg2Rad;

            // 计算位置
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            return center + offset;
        }

        /// <summary>
        /// 弧形生成：在指定角度范围内分布
        /// </summary>
        private static Vector3 GetArcSpawnPosition(Transform[] spawnPoints, Transform centerPoint, int count, int index)
        {
            if (centerPoint == null)
            {
                centerPoint = spawnPoints[0];
            }

            Vector3 center = centerPoint.position;
            float radius = 5f;
            float arcAngle = 120f; // 120度弧形
            float startAngle = -arcAngle / 2f; // 从-60度开始

            // 在弧形范围内均匀分布
            float angleStep = arcAngle / Mathf.Max(1, count - 1);
            float angle = (startAngle + angleStep * index) * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            return center + offset;
        }

        /// <summary>
        /// 中心生成：在中心点位置生成
        /// </summary>
        private static Vector3 GetCenterSpawnPosition(Transform[] spawnPoints, Transform centerPoint)
        {
            if (centerPoint != null)
            {
                return centerPoint.position;
            }

            // 如果没有指定中心点，使用第一个spawn point
            if (spawnPoints.Length > 0)
            {
                return spawnPoints[0].position;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 随机生成：从spawn points中随机选择
        /// </summary>
        private static Vector3 GetRandomSpawnPosition(Transform[] spawnPoints)
        {
            if (spawnPoints.Length == 0) return Vector3.zero;
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }
    }
}

