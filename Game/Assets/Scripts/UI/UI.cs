using System.Collections;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// UI 更新工具类
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// 更新 GameObject 上的所有 DataBinder 组件
        /// </summary>
        /// <param name="gameObject">要更新的 GameObject</param>
        /// <param name="data">数据对象（必须与 DataBinder 上配置的类型匹配）</param>
        public static void Update(GameObject gameObject, object data)
        {
            if (gameObject == null)
            {
                Debug.LogError("[UI.Update] GameObject 为 null");
                return;
            }

            if (data == null)
            {
                Debug.LogError("[UI.Update] 数据为 null");
                return;
            }

            // 检查是否为 List 类型
            if (IsListType(data))
            {
                UpdateListDisplay(gameObject, data);
                return;
            }

            // 获取所有 DataBinder 组件
            DataBinder[] binders = gameObject.GetComponentsInChildren<DataBinder>(true);
            
            if (binders.Length == 0)
            {
                Debug.LogWarning($"[UI.Update] {gameObject.name}: 未找到 DataBinder 组件");
                return;
            }

            // 更新所有绑定器
            foreach (DataBinder binder in binders)
            {
                if (binder != null)
                {
                    binder.UpdateDisplay(data);
                }
            }
        }

        /// <summary>
        /// 检查值是否为 List 类型
        /// </summary>
        private static bool IsListType(object value)
        {
            if (value == null)
                return false;

            System.Type valueType = value.GetType();
            
            // 检查是否实现了 IList 接口（包括 List<T>, Array 等）
            if (valueType.IsArray)
                return true;
            
            if (typeof(IList).IsAssignableFrom(valueType))
            {
                // 排除字符串（虽然实现了 IList，但应该作为普通文本显示）
                if (valueType == typeof(string))
                    return false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 更新 List 类型数据的显示
        /// </summary>
        private static void UpdateListDisplay(GameObject gameObject, object listValue)
        {
            IList list = listValue as IList;
            if (list == null)
            {
                Debug.LogWarning($"[UI.Update] {gameObject.name}: List 值为 null");
                return;
            }

            Transform transform = gameObject.transform;

            // 获取子节点模板（第一个子节点作为模板）
            Transform templateChild = null;
            if (transform.childCount > 0)
            {
                templateChild = transform.GetChild(0);
            }

            if (templateChild == null)
            {
                Debug.LogWarning($"[UI.Update] {gameObject.name}: 没有子节点作为模板");
                return;
            }

            // 确保有足够的子节点来显示所有 List 元素
            int listCount = list.Count;
            int currentChildCount = transform.childCount;

            // 创建或删除子节点以匹配 List 数量
            if (listCount > currentChildCount)
            {
                // 需要创建更多子节点
                for (int i = currentChildCount; i < listCount; i++)
                {
                    GameObject newChild = Object.Instantiate(templateChild.gameObject, transform);
                    newChild.name = $"{templateChild.name}_{i}";
                }
            }
            else if (listCount < currentChildCount)
            {
                // 需要删除多余的子节点（保留模板）
                for (int i = transform.childCount - 1; i >= listCount; i--)
                {
                    Transform childToRemove = transform.GetChild(i);
                    if (childToRemove != templateChild)
                    {
                        if (Application.isPlaying)
                        {
                            Object.Destroy(childToRemove.gameObject);
                        }
                        else
                        {
                            Object.DestroyImmediate(childToRemove.gameObject);
                        }
                    }
                }
            }

            // 更新每个直接子节点的 DataBinder（只使用下一层节点）
            for (int i = 0; i < listCount; i++)
            {
                Transform child = transform.GetChild(i);
                object item = list[i];

                Update(child.gameObject, item);
            }

            // 隐藏或显示模板节点
            if (listCount > 0)
            {
                templateChild.gameObject.SetActive(true);
            }
            else
            {
                templateChild.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 更新指定 GameObject 上的 DataBinder 组件
        /// </summary>
        public static void Update(Component component, object data)
        {
            if (component != null)
            {
                Update(component.gameObject, data);
            }
        }

        /// <summary>
        /// 更新指定 GameObject 上的单个 DataBinder 组件
        /// </summary>
        public static void UpdateSingle(GameObject gameObject, object data)
        {
            if (gameObject == null)
            {
                Debug.LogError("[UI.UpdateSingle] GameObject 为 null");
                return;
            }

            DataBinder binder = gameObject.GetComponent<DataBinder>();
            if (binder != null)
            {
                binder.UpdateDisplay(data);
            }
            else
            {
                Debug.LogWarning($"[UI.UpdateSingle] {gameObject.name}: 未找到 DataBinder 组件");
            }
        }
    }
}

