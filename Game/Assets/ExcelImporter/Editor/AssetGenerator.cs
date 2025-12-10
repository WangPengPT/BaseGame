using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace ExcelImporter
{
    public class AssetGenerator
    {
        public void GenerateAssets(ExcelData data, string className, string outputPath)
        {
            // 确保输出目录存在
            string fullPath = System.IO.Path.Combine(Application.dataPath, outputPath.Replace("Assets/", ""));
            if (!System.IO.Directory.Exists(fullPath))
            {
                System.IO.Directory.CreateDirectory(fullPath);
            }

            // 加载生成的类
            string rowClassName = className + "Row";
            string scriptableObjectClassName = className;
            
            // 等待脚本编译完成
            AssetDatabase.Refresh();
            
            // 等待编译完成（最多等待 5 秒）
            int maxWaitTime = 50; // 50 * 0.1秒 = 5秒
            int waitCount = 0;
            while (EditorApplication.isCompiling && waitCount < maxWaitTime)
            {
                System.Threading.Thread.Sleep(100);
                waitCount++;
            }
            
            // 如果还在编译，强制重新加载
            if (EditorApplication.isCompiling)
            {
                EditorUtility.RequestScriptReload();
                System.Threading.Thread.Sleep(500);
            }
            
            // 查找类型
            Type rowType = FindType(rowClassName);
            Type soType = FindType(scriptableObjectClassName);

            if (rowType == null || soType == null)
            {
                Debug.LogWarning($"无法找到生成的类 ({rowClassName} 或 {scriptableObjectClassName})，请确保脚本已编译。\n" +
                    "如果这是第一次导入，请稍等片刻后重新导入以生成资源。");
                return;
            }

            // 创建 ScriptableObject 实例
            ScriptableObject asset = ScriptableObject.CreateInstance(soType);
            
            // 获取 rows 字段
            FieldInfo rowsField = soType.GetField("rows");
            if (rowsField == null)
            {
                Debug.LogError("无法找到 rows 字段");
                return;
            }

            // 创建列表
            System.Collections.IList rowsList = (System.Collections.IList)Activator.CreateInstance(
                typeof(List<>).MakeGenericType(rowType));

            // 填充数据
            foreach (var dataRow in data.Rows)
            {
                object rowObj = Activator.CreateInstance(rowType);
                
                foreach (var kvp in dataRow)
                {
                    string fieldName = SanitizeFieldName(kvp.Key);
                    FieldInfo field = rowType.GetField(fieldName);
                    
                    if (field != null)
                    {
                        object value = ConvertValue(kvp.Value, field.FieldType);
                        field.SetValue(rowObj, value);
                    }
                }
                
                rowsList.Add(rowObj);
            }

            rowsField.SetValue(asset, rowsList);

            // 保存资源
            string assetPath = System.IO.Path.Combine(outputPath, className + ".asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"成功生成资源: {assetPath}");
        }

        private Type FindType(string typeName)
        {
            foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType("ExcelData." + typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        private string SanitizeFieldName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Field";

            string sanitized = name.Trim();
            string[] parts = sanitized.Split(new[] { ' ', '_', '-', '.', '(', ')', '[', ']' }, 
                System.StringSplitOptions.RemoveEmptyEntries);
            
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            foreach (string part in parts)
            {
                if (part.Length > 0)
                {
                    result.Append(char.ToUpper(part[0]));
                    if (part.Length > 1)
                    {
                        result.Append(part.Substring(1).ToLower());
                    }
                }
            }

            if (result.Length == 0 || !char.IsLetter(result[0]))
            {
                result.Insert(0, "Field");
            }

            string fieldName = result.ToString();
            if (IsCSharpKeyword(fieldName))
            {
                fieldName = "@" + fieldName;
            }

            return fieldName;
        }

        private object ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (targetType.IsValueType)
                    return Activator.CreateInstance(targetType);
                return null;
            }

            value = value.Trim();

            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(int))
            {
                if (int.TryParse(value, out int result))
                    return result;
                return 0;
            }

            if (targetType == typeof(float))
            {
                if (float.TryParse(value, out float result))
                    return result;
                return 0f;
            }

            if (targetType == typeof(bool))
            {
                string lower = value.ToLower();
                return lower == "true" || lower == "1" || lower == "yes";
            }

            return value;
        }

        private bool IsCSharpKeyword(string word)
        {
            string[] keywords = {
                "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
                "char", "checked", "class", "const", "continue", "decimal", "default",
                "delegate", "do", "double", "else", "enum", "event", "explicit",
                "extern", "false", "finally", "fixed", "float", "for", "foreach",
                "goto", "if", "implicit", "in", "int", "interface", "internal",
                "is", "lock", "long", "namespace", "object", "operator",
                "out", "override", "params", "private", "protected", "public", "readonly",
                "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
                "static", "string", "struct", "switch", "this", "throw", "true",
                "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
                "using", "virtual", "void", "volatile", "while"
            };

            return Array.Exists(keywords, k => k == word.ToLower());
        }
    }
}

