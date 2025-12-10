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
            // Ensure field list is also refreshed
            DataBinder binder = (DataBinder)target;
            if (!string.IsNullOrEmpty(binder.dataTypeName))
            {
                _selectedDataType = FindType(binder.dataTypeName);
                RefreshFieldNames();
            }
            // Restore custom method selection
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
            EditorGUILayout.LabelField("Data Binding Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Data type selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Data Type:", GUILayout.Width(100));
            
            // Ensure index synchronization
            if (!string.IsNullOrEmpty(binder.dataTypeName) && _dataTypeNames != null && _dataTypeNames.Length > 0)
            {
                int foundIndex = Array.IndexOf(_dataTypeNames, binder.dataTypeName);
                if (foundIndex >= 0 && foundIndex != _selectedDataTypeIndex)
                {
                    _selectedDataTypeIndex = foundIndex;
                    _selectedDataType = FindType(binder.dataTypeName);
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
                    
                    // Use SerializedProperty to ensure value is saved correctly
                    SerializedProperty dataTypeNameProp = serializedObject.FindProperty("dataTypeName");
                    if (dataTypeNameProp != null)
                    {
                        dataTypeNameProp.stringValue = newTypeName;
                    }
                    else
                    {
                        binder.dataTypeName = newTypeName;
                    }
                    
                    _selectedDataType = FindType(newTypeName);
                    RefreshFieldNames();
                    
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(binder);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No types found in Datas namespace. Please ensure Excel data has been imported and corresponding classes generated.", MessageType.Warning);
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(50)))
            {
                RefreshDataTypeNames();
                if (!string.IsNullOrEmpty(binder.dataTypeName))
                {
                    _selectedDataTypeIndex = Array.IndexOf(_dataTypeNames, binder.dataTypeName);
                    if (_selectedDataTypeIndex < 0) _selectedDataTypeIndex = 0;
                    _selectedDataType = FindType(binder.dataTypeName);
                    RefreshFieldNames();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Ensure _selectedDataType is always up to date
            if (!string.IsNullOrEmpty(binder.dataTypeName) && _selectedDataType == null)
            {
                _selectedDataType = FindType(binder.dataTypeName);
                RefreshFieldNames();
            }

            // Field selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Field Name:", GUILayout.Width(100));
            
            // Force refresh field list if type is selected
            if (!string.IsNullOrEmpty(binder.dataTypeName))
            {
                // Ensure type is loaded
                string cleanTypeName = binder.dataTypeName.Replace(" (Component)", "");
                if (_selectedDataType == null || _selectedDataType.Name != cleanTypeName)
                {
                    _selectedDataType = FindType(binder.dataTypeName);
                    if (_selectedDataType != null)
                    {
                        RefreshFieldNames();
                    }
                }
                
                // If type is loaded, show dropdown selection
                if (_selectedDataType != null)
                {
                    // Ensure field list is refreshed
                    if (_fieldNames == null || _fieldNames.Length == 0)
                    {
                        RefreshFieldNames();
                    }
                    
                    // Always show dropdown selection (even if field list is empty, show "None" option)
                    string[] fieldNamesWithNone;
                    if (_fieldNames != null && _fieldNames.Length > 0)
                    {
                        fieldNamesWithNone = new string[_fieldNames.Length + 1];
                        fieldNamesWithNone[0] = "(None - Bind entire object)";
                        Array.Copy(_fieldNames, 0, fieldNamesWithNone, 1, _fieldNames.Length);
                    }
                    else
                    {
                        // If no fields, only show "None" option
                        fieldNamesWithNone = new string[] { "(None - Bind entire object)" };
                    }
                    
                    // Calculate current index
                    int currentIndex = 0;
                    if (!string.IsNullOrEmpty(binder.fieldName) && _fieldNames != null && _fieldNames.Length > 0)
                    {
                        int foundIndex = Array.IndexOf(_fieldNames, binder.fieldName);
                        if (foundIndex >= 0)
                        {
                            currentIndex = foundIndex + 1; // +1 because first is "None"
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
                        EditorGUILayout.HelpBox($"Type {binder.dataTypeName} has no public fields or properties", MessageType.Warning);
                    }
                }
                else
                {
                    // Type not found, show text input
                    binder.fieldName = EditorGUILayout.TextField(binder.fieldName);
                    EditorGUILayout.HelpBox($"Cannot find type: {binder.dataTypeName}. Please check if it's in Datas namespace or exists as a component.", MessageType.Warning);
                }
            }
            else
            {
                // If type is not selected, show text input
                binder.fieldName = EditorGUILayout.TextField(binder.fieldName);
            }
            EditorGUILayout.EndHorizontal();
            
            if (!string.IsNullOrEmpty(binder.dataTypeName) && string.IsNullOrEmpty(binder.fieldName))
            {
                EditorGUILayout.HelpBox("Empty field name will bind the entire data object", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Custom display method selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Method:", GUILayout.Width(100));
            
            if (_customMethodNames != null && _customMethodNames.Length > 0)
            {
                // Add "None - Use default display" option
                string[] methodNamesWithNone = new string[_customMethodNames.Length + 1];
                methodNamesWithNone[0] = "(None - Use default display)";
                Array.Copy(_customMethodNames, 0, methodNamesWithNone, 1, _customMethodNames.Length);
                
                // Calculate current index
                int currentMethodIndex = 0;
                if (!string.IsNullOrEmpty(binder.customDisplayMethod))
                {
                    int foundIndex = Array.IndexOf(_customMethodNames, binder.customDisplayMethod);
                    if (foundIndex >= 0)
                    {
                        currentMethodIndex = foundIndex + 1; // +1 because first is "None"
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
                // If no custom methods, show text input
                binder.customDisplayMethod = EditorGUILayout.TextField(binder.customDisplayMethod);
            }
            
            if (GUILayout.Button("Refresh", GUILayout.Width(50)))
            {
                RefreshCustomMethodNames();
            }
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(binder.customDisplayMethod))
            {
                EditorGUILayout.HelpBox("Using default display method (automatically determined by UI component type)", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Will use UIFunctions.{binder.customDisplayMethod} method to display", MessageType.Info);
            }

            //EditorGUILayout.Space();
            //EditorGUILayout.LabelField("支持的 UI 组件:", EditorStyles.boldLabel);
            //EditorGUILayout.LabelField("• Text / TextMeshProUGUI: 显示为字符串");
            //EditorGUILayout.LabelField("• Toggle: bool 值控制开关");
            //EditorGUILayout.LabelField("• Slider: float/int 值控制进度");
            //EditorGUILayout.LabelField("• Image: Sprite/Texture2D");
            //EditorGUILayout.LabelField("• RawImage: Texture");

            EditorGUILayout.Space();

            // Debug information
            EditorGUILayout.LabelField("Debug Information", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Data Type:", binder.currentDataType);
            EditorGUILayout.TextField("Field Type:", binder.currentFieldType);
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
            HashSet<string> typeNameSet = new HashSet<string>();
            
            // Get types from Datas namespace
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.Namespace == "Datas" && !type.IsAbstract && !type.IsInterface)
                        {
                            // Exclude Row classes, only show main data classes
                            if (!type.Name.EndsWith("Row"))
                            {
                                if (!typeNameSet.Contains(type.Name))
                                {
                                    typeNames.Add(type.Name);
                                    typeNameSet.Add(type.Name);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore assemblies that cannot be loaded
                }
            }

            // Get component types from current GameObject and parent GameObjects
            // Rule: Only include types that inherit from UIBase or have no namespace
            DataBinder binder = (DataBinder)target;
            if (binder != null && binder.gameObject != null)
            {
                HashSet<Type> componentTypes = new HashSet<Type>();
                Type uiBaseType = typeof(UIBase);
                
                // Collect all GameObjects to check: current node and all parent nodes
                List<GameObject> gameObjectsToCheck = new List<GameObject>();
                gameObjectsToCheck.Add(binder.gameObject);
                
                Transform parent = binder.transform.parent;
                while (parent != null)
                {
                    gameObjectsToCheck.Add(parent.gameObject);
                    parent = parent.parent;
                }
                
                // Check all components on current and parent GameObjects
                foreach (GameObject go in gameObjectsToCheck)
                {
                    Component[] components = go.GetComponents<Component>();
                    foreach (Component comp in components)
                    {
                        if (comp == null || comp.GetType() == typeof(Transform) || comp.GetType() == typeof(DataBinder))
                            continue;
                        
                        Type compType = comp.GetType();
                        
                        // Check if inherits from UIBase
                        if (uiBaseType.IsAssignableFrom(compType) && compType != uiBaseType)
                        {
                            componentTypes.Add(compType);
                        }
                        // Check if has no namespace
                        else if (string.IsNullOrEmpty(compType.Namespace))
                        {
                            // Exclude Unity and System types
                            if (!compType.FullName.StartsWith("UnityEngine.") && 
                                !compType.FullName.StartsWith("System.") &&
                                !compType.FullName.StartsWith("Unity."))
                            {
                                componentTypes.Add(compType);
                            }
                        }
                    }
                }
                
                // Add component types to list
                foreach (Type compType in componentTypes)
                {
                    string typeName = compType.Name;
                    if (!typeNameSet.Contains(typeName))
                    {
                        typeNames.Add($"{typeName} (Component)");
                        typeNameSet.Add(typeName);
                    }
                }
            }

            typeNames.Sort();
            _dataTypeNames = typeNames.ToArray();

            // Restore current selection
            if (!string.IsNullOrEmpty(binder.dataTypeName))
            {
                // Try to find exact match first
                _selectedDataTypeIndex = Array.IndexOf(_dataTypeNames, binder.dataTypeName);
                
                // If not found, try to find without "(Component)" suffix
                if (_selectedDataTypeIndex < 0)
                {
                    for (int i = 0; i < _dataTypeNames.Length; i++)
                    {
                        string name = _dataTypeNames[i];
                        if (name == binder.dataTypeName || name.StartsWith(binder.dataTypeName + " "))
                        {
                            _selectedDataTypeIndex = i;
                            break;
                        }
                    }
                }
                
                if (_selectedDataTypeIndex < 0) _selectedDataTypeIndex = 0;
                
                // Try to find the type
                _selectedDataType = FindType(binder.dataTypeName);
                RefreshFieldNames();
            }
        }

        private void RefreshFieldNames()
        {
            List<string> fieldNames = new List<string>();
            DataBinder binder;

            if (_selectedDataType != null)
            {
                // Check if it's a component type (has "(Component)" suffix in the name)
                binder = (DataBinder)target;
                bool isComponentType = !string.IsNullOrEmpty(binder.dataTypeName) && binder.dataTypeName.Contains(" (Component)");
                
                // Get all public fields
                FieldInfo[] fields = _selectedDataType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (FieldInfo field in fields)
                {
                    // If it's a component type, only show fields declared in this class
                    if (isComponentType)
                    {
                        if (field.DeclaringType == _selectedDataType)
                        {
                            fieldNames.Add(field.Name);
                        }
                    }
                    else
                    {
                        // For Datas namespace types, show all fields
                        fieldNames.Add(field.Name);
                    }
                }

                // Get all public properties
                PropertyInfo[] properties = _selectedDataType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (PropertyInfo property in properties)
                {
                    if (property.CanRead)
                    {
                        // If it's a component type, only show properties declared in this class
                        if (isComponentType)
                        {
                            if (property.DeclaringType == _selectedDataType)
                            {
                                fieldNames.Add(property.Name);
                            }
                        }
                        else
                        {
                            // For Datas namespace types, show all properties
                            fieldNames.Add(property.Name);
                        }
                    }
                }
            }

            fieldNames.Sort();
            _fieldNames = fieldNames.ToArray();

            // Restore current selection
            binder = (DataBinder)target;
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

        private Type FindType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
            
            // Remove "(Component)" suffix if present
            string cleanTypeName = typeName.Replace(" (Component)", "");
            
            // First try Datas namespace
            Type type = FindTypeInDatasNamespace(cleanTypeName);
            if (type != null)
                return type;
            
            // Then try to find in all assemblies (for component types)
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Try with full namespace first
                    type = assembly.GetType(cleanTypeName);
                    if (type != null)
                        return type;
                    
                    // Try without namespace (just class name)
                    Type[] types = assembly.GetTypes();
                    foreach (Type t in types)
                    {
                        if (t.Name == cleanTypeName && !t.IsAbstract && !t.IsInterface)
                        {
                            return t;
                        }
                    }
                }
                catch
                {
                    // Ignore
                }
            }
            
            return null;
        }

        private void RefreshCustomMethodNames()
        {
            List<string> methodNames = new List<string>();
            
            Type uiFunctionsType = typeof(UIFunctions);
            if (uiFunctionsType != null)
            {
                // Get all public static methods
                MethodInfo[] methods = uiFunctionsType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (MethodInfo method in methods)
                {
                    // Check method signature: public static void MethodName(GameObject, T)
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(GameObject))
                    {
                        methodNames.Add(method.Name);
                    }
                }
            }

            methodNames.Sort();
            _customMethodNames = methodNames.ToArray();

            // Restore current selection
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

