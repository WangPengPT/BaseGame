using UnityEngine;
using UnityEditor;
using System.IO;
using ExcelData;

namespace ExcelImporter.Editor
{
    /// <summary>
    /// 批量创建 ExcelData ScriptableObject 资源
    /// </summary>
    public class CreateExcelDataAssets : EditorWindow
    {
        [MenuItem("Tools/Create ExcelData Assets")]
        public static void ShowWindow()
        {
            CreateExcelDataAssets window = GetWindow<CreateExcelDataAssets>("Create ExcelData Assets");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("批量创建 ExcelData 资源", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "此工具会为所有 ExcelData 类创建 ScriptableObject 资源文件。\n" +
                "资源文件将保存在 Assets/Resources/ExcelData 目录下。",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("创建所有资源", GUILayout.Height(30)))
            {
                CreateAllAssets();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "注意：创建的资源文件是空的，需要手动填充数据，\n" +
                "或者使用 Unity Editor 的 Excel Importer 工具导入数据。",
                MessageType.Warning);
        }

        private void CreateAllAssets()
        {
            string outputPath = "Assets/Resources/ExcelData";
            if (!AssetDatabase.IsValidFolder(outputPath))
            {
                string parentPath = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder(parentPath, "ExcelData");
            }

            // 获取所有 ExcelData 命名空间下的 ScriptableObject 类型
            // 遍历所有已加载的程序集，查找 ExcelData 命名空间下的类型
            var types = new System.Collections.Generic.List<System.Type>();
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var assemblyTypes = assembly.GetTypes();
                    foreach (var type in assemblyTypes)
                    {
                        if (type.Namespace == "ExcelData" &&
                            type.IsSubclassOf(typeof(ScriptableObject)) &&
                            typeof(ExcelImporter.IExcelDataTable).IsAssignableFrom(type))
                        {
                            types.Add(type);
                        }
                    }
                }
                catch (System.Reflection.ReflectionTypeLoadException)
                {
                    // 忽略无法加载的程序集
                    continue;
                }
            }

            int createdCount = 0;
            foreach (var type in types)
            {
                if (type != null)
                {
                    string assetPath = $"{outputPath}/{type.Name}.asset";
                    
                    // 检查是否已存在
                    if (File.Exists(assetPath))
                    {
                        Debug.Log($"资源已存在，跳过: {assetPath}");
                        continue;
                    }

                    // 创建资源
                    ScriptableObject asset = ScriptableObject.CreateInstance(type);
                    AssetDatabase.CreateAsset(asset, assetPath);
                    createdCount++;
                    Debug.Log($"创建资源: {assetPath}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("完成", 
                $"成功创建 {createdCount} 个资源文件！\n\n" +
                "现在可以使用 Excel Importer 工具导入数据了。", 
                "确定");
        }
    }
}

