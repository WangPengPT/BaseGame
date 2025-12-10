using System.Collections.Generic;
using UnityEngine;

namespace ExcelImporter
{
    /// <summary>
    /// Excel 数据加载器 - 提供便捷的静态方法访问所有表
    /// </summary>
    public static class ExcelDataLoader
    {
        private static Dictionary<string, IExcelDataTable> _tables;

        /// <summary>
        /// 初始化并加载所有表
        /// </summary>
        public static void Initialize()
        {
            ExcelDataManager.LoadAllTables();
            _tables = ExcelDataManager.GetAllTables();
        }

        /// <summary>
        /// 获取所有已加载的表
        /// </summary>
        public static Dictionary<string, IExcelDataTable> GetAllTables()
        {
            if (_tables == null)
            {
                Initialize();
            }
            return _tables;
        }

        /// <summary>
        /// 根据表名获取数据表
        /// </summary>
        /// <typeparam name="T">表类型</typeparam>
        /// <param name="tableName">表名</param>
        /// <returns>数据表</returns>
        public static T GetTable<T>(string tableName) where T : ScriptableObject, IExcelDataTable
        {
            return ExcelDataManager.GetTable<T>(tableName);
        }

        /// <summary>
        /// 根据类型获取数据表（自动从类型名推断表名）
        /// </summary>
        /// <typeparam name="T">表类型</typeparam>
        /// <returns>数据表</returns>
        public static T GetTable<T>() where T : ScriptableObject, IExcelDataTable
        {
            string tableName = typeof(T).Name;
            return GetTable<T>(tableName);
        }

        /// <summary>
        /// 根据表名获取数据表（使用接口）
        /// </summary>
        public static IExcelDataTable GetTable(string tableName)
        {
            return ExcelDataManager.GetTable(tableName);
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        public static bool HasTable(string tableName)
        {
            if (_tables == null)
            {
                Initialize();
            }
            return _tables.ContainsKey(tableName);
        }

        /// <summary>
        /// 获取所有表名
        /// </summary>
        public static List<string> GetAllTableNames()
        {
            if (_tables == null)
            {
                Initialize();
            }
            return new List<string>(_tables.Keys);
        }
    }
}

