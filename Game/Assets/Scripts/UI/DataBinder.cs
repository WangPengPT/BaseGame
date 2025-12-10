using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// 数据绑定组件 - 在编辑器中选择数据类型、字段和显示方法
    /// </summary>
    public class DataBinder : MonoBehaviour
    {
        [Header("数据配置")]
        [Tooltip("数据类型（Datas 命名空间下的类名）")]
        public string dataTypeName = "";

        [Tooltip("字段名")]
        public string fieldName = "";

        [Header("显示方法")]
        [Tooltip("自定义显示方法名（可选，留空使用默认方法）")]
        public string customDisplayMethod = "";

        [Header("调试信息")]
        [Tooltip("当前绑定的数据类型")]
        [SerializeField]
        public string currentDataType = "";

        [Tooltip("当前绑定的字段类型")]
        [SerializeField]
        public string currentFieldType = "";

        private Type _dataType;
        private FieldInfo _fieldInfo;
        private PropertyInfo _propertyInfo;
        private object _lastData;

        private void OnValidate()
        {
            UpdateTypeInfo();
        }

        private void UpdateTypeInfo()
        {
            if (string.IsNullOrEmpty(dataTypeName))
            {
                currentDataType = "";
                currentFieldType = "";
                return;
            }

            // 查找 Datas 命名空间下的类型
            _dataType = FindTypeInDatasNamespace(dataTypeName);
            if (_dataType != null)
            {
                currentDataType = _dataType.FullName;

                // 查找字段或属性
                if (!string.IsNullOrEmpty(fieldName))
                {
                    _fieldInfo = _dataType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                    _propertyInfo = _dataType.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance);

                    if (_fieldInfo != null)
                    {
                        currentFieldType = _fieldInfo.FieldType.Name;
                    }
                    else if (_propertyInfo != null)
                    {
                        currentFieldType = _propertyInfo.PropertyType.Name;
                    }
                    else
                    {
                        currentFieldType = "未找到";
                    }
                }
            }
            else
            {
                currentDataType = "类型未找到";
            }
        }

        private Type FindTypeInDatasNamespace(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType("Datas." + typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        /// <summary>
        /// 更新显示（由 UI.Update 调用）
        /// </summary>
        public void UpdateDisplay(object data)
        {
            if (data == null)
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: 数据为 null");
                return;
            }

            // 检查数据类型是否匹配
            if (_dataType == null)
            {
                UpdateTypeInfo();
            }

            if (_dataType == null || data.GetType() != _dataType)
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: 数据类型不匹配。期望: {_dataType?.Name}, 实际: {data.GetType().Name}");
                return;
            }

            _lastData = data;

            // 获取字段值
            object fieldValue = GetFieldValue(data);
            if (fieldValue == null && !string.IsNullOrEmpty(fieldName))
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: 无法获取字段 {fieldName} 的值");
                return;
            }

            // 检查是否为 List 类型
            if (IsListType(fieldValue))
            {
                UpdateListDisplay(fieldValue);
                return;
            }

            // 使用自定义方法或默认方法显示
            if (!string.IsNullOrEmpty(customDisplayMethod))
            {
                UseCustomDisplayMethod(fieldValue);
            }
            else
            {
                UseDefaultDisplay(fieldValue);
            }
        }

        private object GetFieldValue(object data)
        {
            if (string.IsNullOrEmpty(fieldName))
                return data;

            if (_fieldInfo != null)
            {
                return _fieldInfo.GetValue(data);
            }
            else if (_propertyInfo != null)
            {
                return _propertyInfo.GetValue(data);
            }

            return null;
        }

        private void UseCustomDisplayMethod(object value)
        {
            // 查找 UIFunctions 类中的静态方法
            Type uiFunctionsType = typeof(UIFunctions);
            MethodInfo method = uiFunctionsType.GetMethod(customDisplayMethod, BindingFlags.Public | BindingFlags.Static);
            
            if (method != null)
            {
                ParameterInfo[] parameters = method.GetParameters();
                // 检查方法签名: (GameObject target, T value)
                if (parameters.Length == 2 && 
                    parameters[0].ParameterType == typeof(GameObject) &&
                    (parameters[1].ParameterType.IsAssignableFrom(value?.GetType() ?? typeof(object)) ||
                     value == null && !parameters[1].ParameterType.IsValueType))
                {
                    method.Invoke(null, new object[] { gameObject, value });
                    return;
                }
                else
                {
                    Debug.LogWarning($"[DataBinder] {gameObject.name}: 自定义方法 {customDisplayMethod} 参数不匹配。期望: (GameObject, {value?.GetType().Name ?? "object"})");
                }
            }
            else
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: 未找到自定义方法 {customDisplayMethod}，请确保方法在 UIFunctions 类中且为 public static");
            }
            
            // 如果自定义方法调用失败，使用默认显示
            UseDefaultDisplay(value);
        }

        private void UseDefaultDisplay(object value)
        {
            // 根据 UI 组件类型自动决定显示方式
            if (TryUpdateText(value))
                return;
            if (TryUpdateToggle(value))
                return;
            if (TryUpdateSlider(value))
                return;
            if (TryUpdateImage(value))
                return;
            if (TryUpdateRawImage(value))
                return;

            Debug.LogWarning($"[DataBinder] {gameObject.name}: 未找到支持的 UI 组件");
        }

        private bool TryUpdateText(object value)
        {
            // 尝试 Text 组件
            Text text = GetComponent<Text>();
            if (text != null)
            {
                text.text = value?.ToString() ?? "";
                return true;
            }

            // 尝试 TextMeshProUGUI
            TMP_Text tmpText = GetComponent<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = value?.ToString() ?? "";
                return true;
            }

            return false;
        }

        private bool TryUpdateToggle(object value)
        {
            Toggle toggle = GetComponent<Toggle>();
            if (toggle != null)
            {
                if (value is bool boolValue)
                {
                    toggle.isOn = boolValue;
                    return true;
                }
                else if (value != null)
                {
                    // 尝试转换
                    if (bool.TryParse(value.ToString(), out bool parsedValue))
                    {
                        toggle.isOn = parsedValue;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryUpdateSlider(object value)
        {
            Slider slider = GetComponent<Slider>();
            if (slider != null)
            {
                if (value is float floatValue)
                {
                    slider.value = floatValue;
                    return true;
                }
                else if (value is int intValue)
                {
                    slider.value = intValue;
                    return true;
                }
                else if (value != null)
                {
                    if (float.TryParse(value.ToString(), out float parsedValue))
                    {
                        slider.value = parsedValue;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryUpdateImage(object value)
        {
            Image image = GetComponent<Image>();
            if (image != null)
            {
                if (value is Sprite sprite)
                {
                    image.sprite = sprite;
                    return true;
                }
                else if (value is Texture2D texture)
                {
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    return true;
                }
            }
            return false;
        }

        private bool TryUpdateRawImage(object value)
        {
            RawImage rawImage = GetComponent<RawImage>();
            if (rawImage != null)
            {
                if (value is Texture texture)
                {
                    rawImage.texture = texture;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查值是否为 List 类型
        /// </summary>
        private bool IsListType(object value)
        {
            if (value == null)
                return false;

            Type valueType = value.GetType();
            
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
        private void UpdateListDisplay(object listValue)
        {
            IList list = listValue as IList;
            if (list == null)
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: List 值为 null");
                return;
            }

            // 获取子节点模板（第一个子节点作为模板）
            Transform templateChild = null;
            if (transform.childCount > 0)
            {
                templateChild = transform.GetChild(0);
            }

            if (templateChild == null)
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: 没有子节点作为模板");
                return;
            }

            // 检查模板子节点是否有 DataBinder 组件（只检查直接子节点，不包括自己）
            DataBinder templateBinder = templateChild.GetComponent<DataBinder>();
            if (templateBinder == null)
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: 模板子节点 {templateChild.name} 没有 DataBinder 组件");
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
                    GameObject newChild = Instantiate(templateChild.gameObject, transform);
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
                            Destroy(childToRemove.gameObject);
                        }
                        else
                        {
                            DestroyImmediate(childToRemove.gameObject);
                        }
                    }
                }
            }

            // 更新每个直接子节点的 DataBinder（只使用下一层节点）
            for (int i = 0; i < listCount; i++)
            {
                Transform child = transform.GetChild(i);
                object item = list[i];

                UI.Update(child.gameObject, item);
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
    }
}

