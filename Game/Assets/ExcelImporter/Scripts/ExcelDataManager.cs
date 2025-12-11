using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelImporter
{
    /// <summary>
    /// Excel 数据管理器，用于运行时读取 Excel 数据
    /// </summary>
    public class ExcelDataManager : MonoBehaviour
    {
        private static ExcelDataManager _instance;
        public static ExcelDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ExcelDataManager");
                    _instance = go.AddComponent<ExcelDataManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private Dictionary<string, ScriptableObject> _loadedAssets = new Dictionary<string, ScriptableObject>();
        private Dictionary<string, IExcelDataTable> _loadedTables = new Dictionary<string, IExcelDataTable>();
        private bool _allTablesLoaded = false;
        private string _resourcesPath = "ExcelData";
        private bool _lazyMode = false;

        /// <summary>
        /// 设置资源路径（相对于 Resources 文件夹）
        /// </summary>
        public static void SetResourcesPath(string path)
        {
            Instance._resourcesPath = path;
        }

        /// <summary>
        /// 加载所有 Excel 数据表（自动发现并加载）
        /// </summary>
        public static void LoadAllTables()
        {
            Instance.LoadAllTablesInternal();
        }

        /// <summary>
        /// 启用懒加载模式：按需加载表，避免一次性 LoadAll。
        /// </summary>
        public static void EnableLazyLoad(bool enabled)
        {
            Instance._lazyMode = enabled;
        }

        /// <summary>
        /// 获取所有已加载的表
        /// </summary>
        public static Dictionary<string, IExcelDataTable> GetAllTables()
        {
            if (!Instance._allTablesLoaded)
            {
                LoadAllTables();
            }
            return Instance._loadedTables;
        }

        /// <summary>
        /// 根据表名获取数据表
        /// </summary>
        public static T GetTable<T>(string tableName) where T : ScriptableObject, IExcelDataTable
        {
            if (!Instance._allTablesLoaded)
            {
                if (Instance._lazyMode)
                {
                    Instance.EnsureTableLoaded(tableName);
                }
                else
                {
                    LoadAllTables();
                }
            }

            if (Instance._loadedTables.TryGetValue(tableName, out IExcelDataTable table))
            {
                return table as T;
            }

            Debug.LogWarning($"表 {tableName} 未找到");
            return null;
        }

        /// <summary>
        /// 根据类型获取数据表（自动从类型名推断表名）
        /// </summary>
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
            if (!Instance._allTablesLoaded)
            {
                if (Instance._lazyMode)
                {
                    Instance.EnsureTableLoaded(tableName);
                }
                else
                {
                    LoadAllTables();
                }
            }

            if (Instance._loadedTables.TryGetValue(tableName, out IExcelDataTable table))
            {
                return table;
            }

            Debug.LogWarning($"表 {tableName} 未找到");
            return null;
        }

        /// <summary>
        /// 加载 Excel 数据资源
        /// </summary>
        /// <typeparam name="T">数据类型（ScriptableObject）</typeparam>
        /// <param name="resourcePath">资源路径（相对于 Resources 文件夹）</param>
        /// <returns>数据对象</returns>
        public T LoadData<T>(string resourcePath) where T : ScriptableObject
        {
            if (_loadedAssets.ContainsKey(resourcePath))
            {
                return _loadedAssets[resourcePath] as T;
            }

            T asset = Resources.Load<T>(resourcePath);
            if (asset != null)
            {
                _loadedAssets[resourcePath] = asset;
            }
            else
            {
                Debug.LogError($"无法加载资源: {resourcePath}");
            }

            return asset;
        }

        /// <summary>
        /// 从 Resources 文件夹加载数据
        /// </summary>
        public static T Load<T>(string resourcePath) where T : ScriptableObject
        {
            return Instance.LoadData<T>(resourcePath);
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            _loadedAssets.Clear();
            _loadedTables.Clear();
            _allTablesLoaded = false;
        }

        private void LoadAllTablesInternal()
        {
            if (_allTablesLoaded)
                return;

            _loadedTables.Clear();

            // 加载 Resources 文件夹下指定路径的所有 ScriptableObject
            ScriptableObject[] allAssets = Resources.LoadAll<ScriptableObject>(_resourcesPath);

            foreach (ScriptableObject asset in allAssets)
            {
                if (asset is IExcelDataTable table)
                {
                    string tableName = table.TableName;
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        _loadedTables[tableName] = table;
                        _loadedAssets[tableName] = asset;
                        Debug.Log($"已加载表: {tableName} (共 {table.Count} 行)");
                    }
                }
            }

            _allTablesLoaded = true;
            Debug.Log($"所有表加载完成，共 {_loadedTables.Count} 个表");
        }

        private void EnsureTableLoaded(string tableName)
        {
            if (_loadedTables.ContainsKey(tableName)) return;

            var asset = Resources.Load<ScriptableObject>($"{_resourcesPath}/{tableName}");
            if (asset is IExcelDataTable table)
            {
                _loadedTables[tableName] = table;
                _loadedAssets[tableName] = asset;
                return;
            }

            // fallback: try one scan to pick up missing
            ScriptableObject[] allAssets = Resources.LoadAll<ScriptableObject>(_resourcesPath);
            foreach (ScriptableObject a in allAssets)
            {
                if (a is IExcelDataTable t && !_loadedTables.ContainsKey(t.TableName))
                {
                    _loadedTables[t.TableName] = t;
                    _loadedAssets[t.TableName] = a;
                }
            }
        }

        private void Awake()
        {
            // 自动加载所有表
            LoadAllTablesInternal();
        }
    }
}

