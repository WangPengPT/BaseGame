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
    /// Data binding component - Select data type, field and display method in editor
    /// </summary>
    public class DataBinder : MonoBehaviour
    {
        [Header("Data Configuration")]
        [Tooltip("Data type (class name in Datas namespace)")]
        public string dataTypeName = "";

        [Tooltip("Field name")]
        public string fieldName = "";

        [Header("Display Method")]
        [Tooltip("Custom display method name (optional, leave empty to use default method)")]
        public string customDisplayMethod = "";

        [Header("Debug Information")]
        [Tooltip("Currently bound data type")]
        [SerializeField]
        public string currentDataType = "";

        [Tooltip("Currently bound field type")]
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

            // Find type in Datas namespace
            _dataType = FindTypeInDatasNamespace(dataTypeName);
            if (_dataType != null)
            {
                currentDataType = _dataType.FullName;

                // Find field or property
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
                        currentFieldType = "Not Found";
                    }
                }
            }
            else
            {
                currentDataType = "Type Not Found";
            }
        }

        private Type FindTypeInDatasNamespace(string typeName)
        {
            // Remove "(Component)" suffix if present
            string cleanTypeName = typeName.Replace(" (Component)", "");
            
            // First try Datas namespace
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType("Datas." + cleanTypeName);
                if (type != null)
                    return type;
            }
            
            // Then try to find in all assemblies (for component types)
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type = assembly.GetType(cleanTypeName);
                    if (type != null)
                        return type;
                }
                catch
                {
                    // Ignore
                }
            }
            
            return null;
        }

        /// <summary>
        /// Update display (called by UITools.Update)
        /// </summary>
        public void UpdateDisplay(object data)
        {
            if (data == null)
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: Data is null");
                return;
            }

            // Check if data type matches
            if (_dataType == null)
            {
                UpdateTypeInfo();
            }

            if (_dataType == null || data.GetType() != _dataType)
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: Data type mismatch. Expected: {_dataType?.Name}, Actual: {data.GetType().Name}");
                return;
            }

            _lastData = data;

            // Get field value
            object fieldValue = GetFieldValue(data);
            if (fieldValue == null && !string.IsNullOrEmpty(fieldName))
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: Cannot get value of field {fieldName}");
                return;
            }

            // Check if it's a List type
            if (IsListType(fieldValue))
            {
                UpdateListDisplay(fieldValue);
                return;
            }

            // Use custom method or default method to display
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
                // Check method signature: (GameObject target, T value)
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
                    Debug.LogWarning($"[DataBinder] {gameObject.name}: Custom method {customDisplayMethod} parameter mismatch. Expected: (GameObject, {value?.GetType().Name ?? "object"})");
                }
            }
            else
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: Custom method {customDisplayMethod} not found, please ensure method is in UIFunctions class and is public static");
            }
            
            // If custom method call fails, use default display
            UseDefaultDisplay(value);
        }

        private void UseDefaultDisplay(object value)
        {
            // Automatically determine display method based on UI component type
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

            Debug.LogWarning($"[DataBinder] {gameObject.name}: No supported UI component found");
        }

        private bool TryUpdateText(object value)
        {
            // Try Text component
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
        /// Check if value is a List type
        /// </summary>
        private bool IsListType(object value)
        {
            if (value == null)
                return false;

            Type valueType = value.GetType();
            
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
        private void UpdateListDisplay(object listValue)
        {
            IList list = listValue as IList;
            if (list == null)
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: List value is null");
                return;
            }

            // Get child node template (first child node as template)
            Transform templateChild = null;
            if (transform.childCount > 0)
            {
                templateChild = transform.GetChild(0);
            }

            if (templateChild == null)
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: No child node as template");
                return;
            }

            // Check if template child node has DataBinder component (only check direct child, not including self)
            DataBinder templateBinder = templateChild.GetComponent<DataBinder>();
            if (templateBinder == null)
            {
                Debug.LogWarning($"[DataBinder] {gameObject.name}: Template child node {templateChild.name} has no DataBinder component");
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
                    GameObject newChild = Instantiate(templateChild.gameObject, transform);
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
                            Destroy(childToRemove.gameObject);
                        }
                        else
                        {
                            DestroyImmediate(childToRemove.gameObject);
                        }
                    }
                }
            }

            // Update DataBinder for each direct child node (only use next level nodes)
            for (int i = 0; i < listCount; i++)
            {
                Transform child = transform.GetChild(i);
                object item = list[i];

                UITools.Update(child.gameObject, item);
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
    }
}

