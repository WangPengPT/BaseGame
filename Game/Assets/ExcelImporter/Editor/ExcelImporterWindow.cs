using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace ExcelImporter
{
    public class ExcelImporterWindow : EditorWindow
    {
        private string documentPath = "";
        private string outputAssetPath = "Assets/Resources/ExcelData";
        private string outputScriptPath = "Assets/Scripts/ExcelData";
        private bool generateScriptableObject = true;
        private bool generateDataClass = true;
        private Vector2 scrollPosition;
        private List<string> foundFiles = new List<string>();

        [MenuItem("Tools/Excel Importer")]
        public static void ShowWindow()
        {
            ExcelImporterWindow window = GetWindow<ExcelImporterWindow>("Excel Importer");
            window.Show();
        }

        private void OnEnable()
        {
            // 自动检测 Document 目录
            documentPath = Path.Combine(Application.dataPath, "../../Document/Config");
            ScanDocumentDirectory();
        }

        private void OnGUI()
        {
            GUILayout.Label("Excel 批量导入工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Document 目录路径
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Document 目录:", GUILayout.Width(120));
            documentPath = EditorGUILayout.TextField(documentPath);
            if (GUILayout.Button("刷新", GUILayout.Width(60)))
            {
                ScanDocumentDirectory();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 输出路径
            outputAssetPath = EditorGUILayout.TextField("资源输出路径:", outputAssetPath);
            outputScriptPath = EditorGUILayout.TextField("脚本输出路径:", outputScriptPath);

            EditorGUILayout.Space();

            // 选项
            generateDataClass = EditorGUILayout.Toggle("生成数据类", generateDataClass);
            generateScriptableObject = EditorGUILayout.Toggle("生成 ScriptableObject", generateScriptableObject);

            EditorGUILayout.Space();

            // 显示找到的文件列表
            if (foundFiles.Count > 0)
            {
                GUILayout.Label($"找到 {foundFiles.Count} 个文件:", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                foreach (string file in foundFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string className = GetClassNameFromFileName(fileName);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"• {fileName}", GUILayout.Width(200));
                    EditorGUILayout.LabelField($"→ {className}", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("未找到 Excel 或 CSV 文件", MessageType.Warning);
            }

            EditorGUILayout.Space();

            // 导入按钮
            GUI.enabled = foundFiles.Count > 0;
            if (GUILayout.Button("导入所有文件", GUILayout.Height(30)))
            {
                ImportAllFiles();
            }
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("工具会自动扫描 Document 目录下的所有 Excel (.xlsx, .xls) 和 CSV 文件。\n" +
                "第一行应为字段名，第二行开始为数据。\n" +
                "类名将使用文件名（去掉扩展名）。", MessageType.Info);
        }

        private void ScanDocumentDirectory()
        {
            foundFiles.Clear();
            if (Directory.Exists(documentPath))
            {
                string[] extensions = { "*.xlsx", "*.xls", "*.csv" };
                foreach (string extension in extensions)
                {
                    foundFiles.AddRange(Directory.GetFiles(documentPath, extension, SearchOption.TopDirectoryOnly));
                }
                foundFiles = foundFiles.Distinct().OrderBy(f => f).ToList();
            }
        }

        private string GetClassNameFromFileName(string fileName)
        {
            // 去掉扩展名，转换为合法的类名
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            return SanitizeClassName(nameWithoutExt);
        }

        private string SanitizeClassName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "ExcelData";

            // 移除空格和特殊字符，保留中文和字母数字
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            bool needUpperCase = true;

            foreach (char c in name)
            {
                // 如果是字母、数字、中文或下划线，保留
                if (char.IsLetterOrDigit(c) || c == '_' || (c >= 0x4E00 && c <= 0x9FFF)) // 中文Unicode范围
                {
                    if (needUpperCase && char.IsLetter(c))
                    {
                        result.Append(char.ToUpper(c));
                        needUpperCase = false;
                    }
                    else
                    {
                        result.Append(c);
                        needUpperCase = false;
                    }
                }
                else if (c == ' ' || c == '-' || c == '.')
                {
                    // 空格、横线、点号后需要大写
                    needUpperCase = true;
                }
            }

            // 确保以字母、下划线或中文开头（C# 类名规则）
            if (result.Length == 0)
            {
                return "ExcelData";
            }

            char firstChar = result[0];
            if (!char.IsLetter(firstChar) && firstChar != '_' && !(firstChar >= 0x4E00 && firstChar <= 0x9FFF))
            {
                result.Insert(0, "Data");
            }

            return result.ToString();
        }

        private void ImportAllFiles()
        {
            if (foundFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到可导入的文件", "确定");
                return;
            }

            int successCount = 0;
            int failCount = 0;
            List<string> failedFiles = new List<string>();

            try
            {
                ExcelReader reader = new ExcelReader();
                ClassGenerator classGenerator = new ClassGenerator();
                AssetGenerator assetGenerator = new AssetGenerator();

                // 确保输出目录存在
                string scriptDir = Path.Combine(Application.dataPath, outputScriptPath.Replace("Assets/", ""));
                if (!Directory.Exists(scriptDir))
                {
                    Directory.CreateDirectory(scriptDir);
                }

                string assetDir = Path.Combine(Application.dataPath, outputAssetPath.Replace("Assets/", ""));
                if (!Directory.Exists(assetDir))
                {
                    Directory.CreateDirectory(assetDir);
                }

                // 批量导入所有文件
                for (int i = 0; i < foundFiles.Count; i++)
                {
                    string filePath = foundFiles[i];
                    string fileName = Path.GetFileName(filePath);
                    string className = GetClassNameFromFileName(fileName);

                    try
                    {
                        float progress = (float)i / foundFiles.Count;
                        EditorUtility.DisplayProgressBar("导入 Excel", $"正在处理: {fileName} ({i + 1}/{foundFiles.Count})", progress);

                        // 读取文件
                        ExcelData data = reader.ReadExcel(filePath);

                        if (data == null || data.Rows.Count == 0)
                        {
                            Debug.LogWarning($"文件 {fileName} 为空或格式不正确，跳过");
                            failCount++;
                            failedFiles.Add(fileName);
                            continue;
                        }

                        // 生成 C# 类
                        if (generateDataClass)
                        {
                            string classCode = classGenerator.GenerateClass(className, data);
                            string scriptPath = Path.Combine(scriptDir, className + ".cs");
                            File.WriteAllText(scriptPath, classCode);
                        }

                        successCount++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"导入文件 {fileName} 失败: {e.Message}");
                        failCount++;
                        failedFiles.Add(fileName);
                    }
                }

                EditorUtility.DisplayProgressBar("导入 Excel", "等待脚本编译...", 0.9f);
                
                // 刷新并等待脚本编译完成
                AssetDatabase.Refresh();
                int waitCount = 0;
                while (EditorApplication.isCompiling && waitCount < 100)
                {
                    System.Threading.Thread.Sleep(100);
                    waitCount++;
                }

                // 生成 ScriptableObject 资源
                if (generateScriptableObject)
                {
                    for (int i = 0; i < foundFiles.Count; i++)
                    {
                        string filePath = foundFiles[i];
                        string fileName = Path.GetFileName(filePath);
                        string className = GetClassNameFromFileName(fileName);

                        // 跳过失败的文件
                        if (failedFiles.Contains(fileName))
                            continue;

                        try
                        {
                            float progress = 0.9f + (float)i / foundFiles.Count * 0.1f;
                            EditorUtility.DisplayProgressBar("导入 Excel", $"正在生成资源: {fileName}", progress);

                            ExcelData data = reader.ReadExcel(filePath);
                            assetGenerator.GenerateAssets(data, className, outputAssetPath);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"生成资源 {fileName} 失败: {e.Message}");
                        }
                    }
                }

                EditorUtility.DisplayProgressBar("导入 Excel", "完成", 1f);
                EditorUtility.ClearProgressBar();

                // 显示结果
                string message = $"导入完成！\n成功: {successCount} 个\n失败: {failCount} 个";
                if (failedFiles.Count > 0)
                {
                    message += $"\n\n失败的文件:\n{string.Join("\n", failedFiles)}";
                }
                EditorUtility.DisplayDialog("导入结果", message, "确定");

                // 刷新资源
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("错误", $"批量导入失败: {e.Message}", "确定");
                Debug.LogError($"Excel 批量导入错误: {e}");
            }
        }
    }
}

