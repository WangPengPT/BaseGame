using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace UI.Editor
{
    [CustomEditor(typeof(DataBinder))]
    [CanEditMultipleObjects]
    public class DataBinderEditor : UnityEditor.Editor
    {
        private string[] _dataTypeNames;
        private string[] _fieldNames;
        private string[] _customMethodNames;
        private int _selectedDataTypeIndex = 0;
        private int _selectedFieldIndex = 0;
        private int _selectedMethodIndex = 0;
        private Type _selectedDataType;

        private void OnEnable()
        {
            RefreshDataTypeNames();
            RefreshCustomMethodNames();
            // 确保字段列表也被刷新
            DataBinder binder = (DataBinder)target;
            if (!string.IsNullOrEmpty(binder.dataTypeName))
            {
                _selectedDataType = FindTypeInDatasNamespace(binder.dataTypeName);
                RefreshFieldNames();
            }
            // 恢复自定义方法选择
            if (!string.IsNullOrEmpty(binder.customDisplayMethod))
            {
                _selectedMethodIndex = Array.IndexOf(_customMethodNames, binder.customDisplayMethod);
                if (_selectedMethodIndex < 0) _selectedMethodIndex = 0;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DataBinder binder = (DataBinder)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("数据绑定配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 数据类型选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("数据类型:", GUILayout.Width(100));
            
            // 确保索引同步
            if (!string.IsNullOrEmpty(binder.dataTypeName) && _dataTypeNames != null && _dataTypeNames.Length > 0)
            {
                int foundIndex = Array.IndexOf(_dataTypeNames, binder.dataTypeName);
                if (foundIndex >= 0 && foundIndex != _selectedDataTypeIndex)
                {
                    _selectedDataTypeIndex = foundIndex;
                    _selectedDataType = FindTypeInDatasNamespace(binder.dataTypeName);
                    RefreshFieldNames();
                }
            }
            
            if (_dataTypeNames != null && _dataTypeNames.Length > 0)
            {
                int newIndex = EditorGUILayout.Popup(_selectedDataTypeIndex, _dataTypeNames);
                if (newIndex != _selectedDataTypeIndex || string.IsNullOrEmpty(binder.dataTypeName))
                {
                    _selectedDataTypeIndex = newIndex;
                    string newTypeName = _dataTypeNames[newIndex];
                    
                    // 使用 SerializedProperty 来确保值被正确保存
                    SerializedProperty dataTypeNameProp = serializedObject.FindProperty("dataTypeName");
                    if (dataTypeNameProp != null)
                    {
                        dataTypeNameProp.stringValue = newTypeName;
                    }
                    else
                    {
                        binder.dataTypeName = newTypeName;
                    }
                    
                    _selectedDataType = FindTypeInDatasNamespace(newTypeName);
                    RefreshFieldNames();
                    
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(binder);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("未找到 Datas 命名空间下的类型。请确保已导入 Excel 数据并生成对应的类。", MessageType.Warning);
            }

            if (GUILayout.Button("刷新", GUILayout.Width(50)))
            {
                RefreshDataTypeNames();
                if (!string.IsNullOrEmpty(binder.dataTypeName))
                {
                    _selectedDataTypeIndex = Array.IndexOf(_dataTypeNames, binder.dataTypeName);
                    if (_selectedDataTypeIndex < 0) _selectedDataTypeIndex = 0;
                    _selectedDataType = FindTypeInDatasNamespace(binder.dataTypeName);
                    RefreshFieldNames();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // 确保 _selectedDataType 始终是最新的
            if (!string.IsNullOrEmpty(binder.dataTypeName) && _selectedDataType == null)
            {
                _selectedDataType = FindTypeInDatasNamespace(binder.dataTypeName);
                RefreshFieldNames();
            }

            // 字段选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("字段名:", GUILayout.Width(100));
            
            // 强制刷新字段列表（如果类型已选择）
            if (!string.IsNullOrEmpty(binder.dataTypeName))
            {
                // 确保类型已加载
                if (_selectedDataType == null || _selectedDataType.Name != binder.dataTypeName)
                {
                    _selectedDataType = FindTypeInDatasNamespace(binder.dataTypeName);
                    if (_selectedDataType != null)
                    {
                        RefreshFieldNames();
                    }
                }
                
                // 如果类型已加载，显示下拉选择
                if (_selectedDataType != null)
                {
                    // 确保字段列表已刷新
                    if (_fieldNames == null || _fieldNames.Length == 0)
                    {
                        RefreshFieldNames();
                    }
                    
                    // 总是显示下拉选择（即使字段列表为空，也显示"无字段"选项）
                    string[] fieldNamesWithNone;
                    if (_fieldNames != null && _fieldNames.Length > 0)
                    {
                        fieldNamesWithNone = new string[_fieldNames.Length + 1];
                        fieldNamesWithNone[0] = "(无 - 绑定整个对象)";
                        Array.Copy(_fieldNames, 0, fieldNamesWithNone, 1, _fieldNames.Length);
                    }
                    else
                    {
                        // 如果没有字段，只显示"无"选项
                        fieldNamesWithNone = new string[] { "(无 - 绑定整个对象)" };
                    }
                    
                    // 计算当前索引
                    int currentIndex = 0;
                    if (!string.IsNullOrEmpty(binder.fieldName) && _fieldNames != null && _fieldNames.Length > 0)
                    {
                        int foundIndex = Array.IndexOf(_fieldNames, binder.fieldName);
                        if (foundIndex >= 0)
                        {
                            currentIndex = foundIndex + 1; // +1 因为第一个是"无"
                        }
                    }
                    
                    int newFieldIndex = EditorGUILayout.Popup(currentIndex, fieldNamesWithNone);
                    if (newFieldIndex != currentIndex)
                    {
                        if (newFieldIndex == 0)
                        {
                            binder.fieldName = "";
                            _selectedFieldIndex = 0;
                        }
                        else if (_fieldNames != null && newFieldIndex - 1 < _fieldNames.Length)
                        {
                            binder.fieldName = _fieldNames[newFieldIndex - 1];
                            _selectedFieldIndex = newFieldIndex - 1;
                        }
                        EditorUtility.SetDirty(binder);
                    }
                    
                    if (_fieldNames == null || _fieldNames.Length == 0)
                    {
                        EditorGUILayout.HelpBox($"类型 {binder.dataTypeName} 没有公共字段或属性", MessageType.Warning);
                    }
                }
                else
                {
                    // 类型未找到，显示文本输入框
                    binder.fieldName = EditorGUILayout.TextField(binder.fieldName);
                    EditorGUILayout.HelpBox($"无法找到类型: {binder.dataTypeName}，请检查命名空间是否为 Datas", MessageType.Warning);
                }
            }
            else
            {
                // 如果类型未选择，显示文本输入框
                binder.fieldName = EditorGUILayout.TextField(binder.fieldName);
            }
            EditorGUILayout.EndHorizontal();
            
            if (!string.IsNullOrEmpty(binder.dataTypeName) && string.IsNullOrEmpty(binder.fieldName))
            {
                EditorGUILayout.HelpBox("留空字段名将绑定整个数据对象", MessageType.Info);
            }

            EditorGUILayout.Space();

            // 自定义显示方法选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("自定义方法:", GUILayout.Width(100));
            
            if (_customMethodNames != null && _customMethodNames.Length > 0)
            {
                // 添加"无 - 使用默认显示"选项
                string[] methodNamesWithNone = new string[_customMethodNames.Length + 1];
                methodNamesWithNone[0] = "(无 - 使用默认显示)";
                Array.Copy(_customMethodNames, 0, methodNamesWithNone, 1, _customMethodNames.Length);
                
                // 计算当前索引
                int currentMethodIndex = 0;
                if (!string.IsNullOrEmpty(binder.customDisplayMethod))
                {
                    int foundIndex = Array.IndexOf(_customMethodNames, binder.customDisplayMethod);
                    if (foundIndex >= 0)
                    {
                        currentMethodIndex = foundIndex + 1; // +1 因为第一个是"无"
                    }
                }
                
                int newMethodIndex = EditorGUILayout.Popup(currentMethodIndex, methodNamesWithNone);
                if (newMethodIndex != currentMethodIndex)
                {
                    if (newMethodIndex == 0)
                    {
                        binder.customDisplayMethod = "";
                        _selectedMethodIndex = 0;
                    }
                    else
                    {
                        binder.customDisplayMethod = _customMethodNames[newMethodIndex - 1];
                        _selectedMethodIndex = newMethodIndex - 1;
                    }
                    EditorUtility.SetDirty(binder);
                }
            }
            else
            {
                // 如果没有自定义方法，显示文本输入框
                binder.customDisplayMethod = EditorGUILayout.TextField(binder.customDisplayMethod);
            }
            
            if (GUILayout.Button("刷新", GUILayout.Width(50)))
            {
                RefreshCustomMethodNames();
            }
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(binder.customDisplayMethod))
            {
                EditorGUILayout.HelpBox("使用默认显示方法（根据 UI 组件类型自动决定）", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"将使用 UIFunctions.{binder.customDisplayMethod} 方法显示", MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("支持的 UI 组件:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Text / TextMeshProUGUI: 显示为字符串");
            EditorGUILayout.LabelField("• Toggle: bool 值控制开关");
            EditorGUILayout.LabelField("• Slider: float/int 值控制进度");
            EditorGUILayout.LabelField("• Image: Sprite/Texture2D");
            EditorGUILayout.LabelField("• RawImage: Texture");

            EditorGUILayout.Space();

            // 显示调试信息
            EditorGUILayout.LabelField("调试信息", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("数据类型:", binder.currentDataType);
            EditorGUILayout.TextField("字段类型:", binder.currentFieldType);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(binder);
            }
        }

        private void RefreshDataTypeNames()
        {
            List<string> typeNames = new List<string>();
            
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.Namespace == "Datas" && !type.IsAbstract && !type.IsInterface)
                        {
                            // 排除 Row 类，只显示主数据类
                            if (!type.Name.EndsWith("Row"))
                            {
                                typeNames.Add(type.Name);
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略无法加载的程序集
                }
            }

            typeNames.Sort();
            _dataTypeNames = typeNames.ToArray();

            // 恢复当前选择
            DataBinder binder = (DataBinder)target;
            if (!string.IsNullOrEmpty(binder.dataTypeName))
            {
                _selectedDataTypeIndex = Array.IndexOf(_dataTypeNames, binder.dataTypeName);
                if (_selectedDataTypeIndex < 0) _selectedDataTypeIndex = 0;
                _selectedDataType = FindTypeInDatasNamespace(binder.dataTypeName);
                RefreshFieldNames();
            }
        }

        private void RefreshFieldNames()
        {
            List<string> fieldNames = new List<string>();
            
            if (_selectedDataType != null)
            {
                // 获取所有公共字段
                FieldInfo[] fields = _selectedDataType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    fieldNames.Add(field.Name);
                }

                // 获取所有公共属性
                PropertyInfo[] properties = _selectedDataType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo property in properties)
                {
                    if (property.CanRead)
                    {
                        fieldNames.Add(property.Name);
                    }
                }
            }

            fieldNames.Sort();
            _fieldNames = fieldNames.ToArray();

            // 恢复当前选择
            DataBinder binder = (DataBinder)target;
            if (!string.IsNullOrEmpty(binder.fieldName))
            {
                _selectedFieldIndex = Array.IndexOf(_fieldNames, binder.fieldName);
                if (_selectedFieldIndex < 0) _selectedFieldIndex = 0;
            }
            else
            {
                _selectedFieldIndex = 0;
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

        private void RefreshCustomMethodNames()
        {
            List<string> methodNames = new List<string>();
            
            Type uiFunctionsType = typeof(UIFunctions);
            if (uiFunctionsType != null)
            {
                // 获取所有公共静态方法
                MethodInfo[] methods = uiFunctionsType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (MethodInfo method in methods)
                {
                    // 检查方法签名: public static void MethodName(GameObject, T)
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(GameObject))
                    {
                        methodNames.Add(method.Name);
                    }
                }
            }

            methodNames.Sort();
            _customMethodNames = methodNames.ToArray();

            // 恢复当前选择
            DataBinder binder = (DataBinder)target;
            if (!string.IsNullOrEmpty(binder.customDisplayMethod))
            {
                _selectedMethodIndex = Array.IndexOf(_customMethodNames, binder.customDisplayMethod);
                if (_selectedMethodIndex < 0) _selectedMethodIndex = 0;
            }
            else
            {
                _selectedMethodIndex = 0;
            }
        }
    }
}

