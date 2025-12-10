using System.Collections;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// UI update utility class
    /// </summary>
    public static class UITools
    {
        /// <summary>
        /// Update all DataBinder components on GameObject
        /// </summary>
        /// <param name="gameObject">GameObject to update</param>
        /// <param name="data">Data object (must match the type configured on DataBinder)</param>
        public static void Update(GameObject gameObject, object data)
        {
            if (gameObject == null)
            {
                Debug.LogError("[UITools.Update] GameObject is null");
                return;
            }

            if (data == null)
            {
                Debug.LogError("[UITools.Update] Data is null");
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
                Debug.LogWarning($"[UITools.Update] {gameObject.name}: No DataBinder component found");
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
            
            // Check if implements IList interface (including List<T>, Array, etc.)
            if (valueType.IsArray)
                return true;
            
            if (typeof(IList).IsAssignableFrom(valueType))
            {
                // Exclude string (although it implements IList, it should be displayed as normal text)
                if (valueType == typeof(string))
                    return false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update display for List type data
        /// </summary>
        private static void UpdateListDisplay(GameObject gameObject, object listValue)
        {
            IList list = listValue as IList;
            if (list == null)
            {
                Debug.LogWarning($"[UITools.Update] {gameObject.name}: List value is null");
                return;
            }

            Transform transform = gameObject.transform;

            // Get child node template (first child node as template)
            Transform templateChild = null;
            if (transform.childCount > 0)
            {
                templateChild = transform.GetChild(0);
            }

            if (templateChild == null)
            {
                Debug.LogWarning($"[UITools.Update] {gameObject.name}: No child node as template");
                return;
            }

            // Ensure there are enough child nodes to display all List elements
            int listCount = list.Count;
            int currentChildCount = transform.childCount;

            // Create or delete child nodes to match List count
            if (listCount > currentChildCount)
            {
                // Need to create more child nodes
                for (int i = currentChildCount; i < listCount; i++)
                {
                    GameObject newChild = Object.Instantiate(templateChild.gameObject, transform);
                    newChild.name = $"{templateChild.name}_{i}";
                }
            }
            else if (listCount < currentChildCount)
            {
                // Need to delete excess child nodes (keep template)
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

            // Update DataBinder for each direct child node (only use next level nodes)
            for (int i = 0; i < listCount; i++)
            {
                Transform child = transform.GetChild(i);
                object item = list[i];

                Update(child.gameObject, item);
            }

            // Hide or show template node
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
        /// Update DataBinder component on specified GameObject
        /// </summary>
        public static void Update(Component component, object data)
        {
            if (component != null)
            {
                Update(component.gameObject, data);
            }
        }

        /// <summary>
        /// Update single DataBinder component on specified GameObject
        /// </summary>
        public static void UpdateSingle(GameObject gameObject, object data)
        {
            if (gameObject == null)
            {
                Debug.LogError("[UITools.UpdateSingle] GameObject is null");
                return;
            }

            DataBinder binder = gameObject.GetComponent<DataBinder>();
            if (binder != null)
            {
                binder.UpdateDisplay(data);
            }
            else
            {
                Debug.LogWarning($"[UITools.UpdateSingle] {gameObject.name}: No DataBinder component found");
            }
        }
    }
}

