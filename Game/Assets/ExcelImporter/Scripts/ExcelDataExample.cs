using UnityEngine;
using System.Collections.Generic;

namespace ExcelImporter
{
    /// <summary>
    /// Excel 数据使用示例
    /// 这个脚本展示了如何在运行时读取和使用 Excel 数据
    /// </summary>
    public class ExcelDataExample : MonoBehaviour
    {
        private void Start()
        {
            // 示例 1: 自动加载所有表
            LoadAllTablesExample();

            // 示例 2: 根据表名获取特定表
            // GetTableByNameExample();

            // 示例 3: 遍历所有表
            // IterateAllTablesExample();
        }

        /// <summary>
        /// 示例 1: 自动加载所有表（推荐方式）
        /// </summary>
        private void LoadAllTablesExample()
        {
            Debug.Log("=== 自动加载所有表示例 ===");

            // 方式 1: 使用 ExcelDataLoader（最简单）
            ExcelDataLoader.Initialize();
            var allTables = ExcelDataLoader.GetAllTables();
            
            Debug.Log($"已加载 {allTables.Count} 个表:");
            foreach (var kvp in allTables)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value.Count} 行数据");
            }

            // 方式 2: 使用 ExcelDataManager
            // ExcelDataManager.LoadAllTables();
            // var tables = ExcelDataManager.GetAllTables();
        }

        /// <summary>
        /// 示例 2: 根据表名获取特定表
        /// </summary>
        private void GetTableByNameExample()
        {
            Debug.Log("=== 根据表名获取表示例 ===");

            // 假设有一个表叫 "PlayerData"
            // 注意：需要根据实际生成的类名替换
            /*
            // 方式 1: 使用泛型（类型安全）
            PlayerData playerData = ExcelDataLoader.GetTable<PlayerData>("PlayerData");
            if (playerData != null)
            {
                Debug.Log($"PlayerData 表有 {playerData.Count} 行");
                foreach (var row in playerData.rows)
                {
                    Debug.Log($"  ID: {row.Id}, Name: {row.Name}");
                }
            }

            // 方式 2: 使用接口（通用方式）
            IExcelDataTable table = ExcelDataLoader.GetTable("PlayerData");
            if (table != null)
            {
                Debug.Log($"表 {table.TableName} 有 {table.Count} 行");
            }
            */
        }

        /// <summary>
        /// 示例 3: 遍历所有表并处理数据
        /// </summary>
        private void IterateAllTablesExample()
        {
            Debug.Log("=== 遍历所有表示例 ===");

            var allTables = ExcelDataLoader.GetAllTables();
            
            foreach (var kvp in allTables)
            {
                string tableName = kvp.Key;
                IExcelDataTable table = kvp.Value;
                
                Debug.Log($"处理表: {tableName} (共 {table.Count} 行)");
                
                // 根据表名进行不同的处理
                switch (tableName)
                {
                    case "PlayerData":
                        // 处理玩家数据
                        // PlayerData playerData = table as PlayerData;
                        // ProcessPlayerData(playerData);
                        break;
                    case "ItemData":
                        // 处理物品数据
                        // ItemData itemData = table as ItemData;
                        // ProcessItemData(itemData);
                        break;
                    default:
                        Debug.Log($"  未知表类型: {tableName}");
                        break;
                }
            }
        }

        /// <summary>
        /// 示例 4: 检查表是否存在
        /// </summary>
        private void CheckTableExistsExample()
        {
            if (ExcelDataLoader.HasTable("PlayerData"))
            {
                Debug.Log("PlayerData 表存在");
            }
            else
            {
                Debug.LogWarning("PlayerData 表不存在");
            }
        }

        /// <summary>
        /// 示例 5: 获取所有表名
        /// </summary>
        private void GetAllTableNamesExample()
        {
            List<string> tableNames = ExcelDataLoader.GetAllTableNames();
            Debug.Log($"所有表名: {string.Join(", ", tableNames)}");
        }
    }
}
