using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelImporter
{
    public class ClassGenerator
    {
        public string GenerateClass(string className, ExcelData data)
        {
            if (data.Headers.Count == 0)
            {
                throw new Exception("Excel 数据没有表头");
            }

            StringBuilder sb = new StringBuilder();

            // 命名空间和引用
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("namespace ExcelData");
            sb.AppendLine("{");
            sb.AppendLine();

            // 数据行类
            sb.AppendLine("    [Serializable]");
            sb.AppendLine($"    public class {className}Row");
            sb.AppendLine("    {");

            // 生成字段
            foreach (string header in data.Headers)
            {
                string fieldName = SanitizeFieldName(header);
                string fieldType = InferFieldType(data, header);
                sb.AppendLine($"        public {fieldType} {fieldName};");
            }

            sb.AppendLine("    }");
            sb.AppendLine();

            // ScriptableObject 类
            sb.AppendLine($"    [CreateAssetMenu(fileName = \"{className}\", menuName = \"ExcelData/{className}\")]");
            sb.AppendLine($"    public class {className} : ScriptableObject, ExcelImporter.IExcelDataTable");
            sb.AppendLine("    {");
            sb.AppendLine($"        public List<{className}Row> rows = new List<{className}Row>();");
            sb.AppendLine();

            // 添加查找方法 - 根据索引
            sb.AppendLine($"        public {className}Row GetRow(int index)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (index >= 0 && index < rows.Count)");
            sb.AppendLine("                return rows[index];");
            sb.AppendLine("            return null;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 添加根据 ID 查找的方法
            string idFieldName = FindIdField(data);
            if (!string.IsNullOrEmpty(idFieldName))
            {
                string idFieldType = InferFieldType(data, GetOriginalHeaderName(data, idFieldName));
                GenerateGetRowByIdMethod(sb, className, idFieldName, idFieldType);
            }

            sb.AppendLine($"        public int Count => rows.Count;");
            sb.AppendLine();

            sb.AppendLine($"        public string TableName => \"{className}\";");
            sb.AppendLine();

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string SanitizeFieldName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Field";

            // 移除空格和特殊字符，转换为 PascalCase
            string sanitized = name.Trim();

            // 替换空格为下划线，然后转换为 PascalCase
            string[] parts = sanitized.Split(new[] { ' ', '_', '-', '.', '(', ')', '[', ']' },
                StringSplitOptions.RemoveEmptyEntries);

            StringBuilder result = new StringBuilder();
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

            // 确保以字母开头
            if (result.Length == 0 || !char.IsLetter(result[0]))
            {
                result.Insert(0, "Field");
            }

            // 移除 C# 关键字
            string fieldName = result.ToString();
            if (IsCSharpKeyword(fieldName))
            {
                fieldName = "@" + fieldName;
            }

            return fieldName;
        }

        private string InferFieldType(ExcelData data, string header)
        {
            if (data.Rows.Count == 0)
                return "string";

            // 检查所有行的数据类型
            bool allInt = true;
            bool allFloat = true;
            bool allBool = true;

            foreach (var row in data.Rows)
            {
                if (!row.ContainsKey(header))
                    continue;

                string value = row[header].Trim();

                if (string.IsNullOrEmpty(value))
                    continue;

                // 检查是否为整数
                if (allInt && !int.TryParse(value, out _))
                    allInt = false;

                // 检查是否为浮点数
                if (allFloat && !float.TryParse(value, out _))
                    allFloat = false;

                // 检查是否为布尔值
                if (allBool)
                {
                    string lower = value.ToLower();
                    if (lower != "true" && lower != "false" && lower != "1" && lower != "0" &&
                        lower != "yes" && lower != "no")
                        allBool = false;
                }
            }

            if (allBool && data.Rows.Any(r => r.ContainsKey(header) && !string.IsNullOrEmpty(r[header])))
                return "bool";
            if (allInt)
                return "int";
            if (allFloat)
                return "float";

            return "string";
        }

        private bool IsCSharpKeyword(string word)
        {
            string[] keywords = {
                "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
                "char", "checked", "class", "const", "continue", "decimal", "default",
                "delegate", "do", "double", "else", "enum", "event", "explicit",
                "extern", "false", "finally", "fixed", "float", "for", "foreach",
                "goto", "if", "implicit", "in", "int", "interface", "internal",
                "is", "lock", "long", "namespace", "new", "null", "object", "operator",
                "out", "override", "params", "private", "protected", "public", "readonly",
                "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
                "static", "string", "struct", "switch", "this", "throw", "true",
                "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
                "using", "virtual", "void", "volatile", "while"
            };

            return Array.Exists(keywords, k => k == word.ToLower());
        }

        /// <summary>
        /// 查找 ID 字段（支持 id, Id, ID 等）
        /// </summary>
        private string FindIdField(ExcelData data)
        {
            // 优先查找的 ID 字段名（不区分大小写）
            string[] idFieldNames = { "id", "Id", "ID" };

            foreach (string header in data.Headers)
            {
                string fieldName = SanitizeFieldName(header);
                string lowerFieldName = fieldName.ToLower();

                // 检查是否是 ID 字段
                foreach (string idName in idFieldNames)
                {
                    if (lowerFieldName == idName.ToLower())
                    {
                        return fieldName;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 根据原始表头名获取字段名
        /// </summary>
        private string GetOriginalHeaderName(ExcelData data, string sanitizedFieldName)
        {
            foreach (string header in data.Headers)
            {
                if (SanitizeFieldName(header) == sanitizedFieldName)
                {
                    return header;
                }
            }
            return null;
        }

        /// <summary>
        /// 生成根据 ID 查找的方法
        /// </summary>
        private void GenerateGetRowByIdMethod(StringBuilder sb, string className, string idFieldName, string idFieldType)
        {
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// 根据 {idFieldName} 查找数据行");
            sb.AppendLine($"        /// </summary>");

            if (idFieldType == "int")
            {
                sb.AppendLine($"        public {className}Row GetRowById(int {idFieldName.ToLower()})");
                sb.AppendLine("        {");
                sb.AppendLine($"            return rows.FirstOrDefault(r => r.{idFieldName} == {idFieldName.ToLower()});");
                sb.AppendLine("        }");
            }
            else if (idFieldType == "string")
            {
                sb.AppendLine($"        public {className}Row GetRowById(string {idFieldName.ToLower()})");
                sb.AppendLine("        {");
                sb.AppendLine($"            if (string.IsNullOrEmpty({idFieldName.ToLower()}))");
                sb.AppendLine("                return null;");
                sb.AppendLine($"            return rows.FirstOrDefault(r => r.{idFieldName} == {idFieldName.ToLower()});");
                sb.AppendLine("        }");
            }
            else
            {
                // 对于其他类型，也生成方法
                sb.AppendLine($"        public {className}Row GetRowById({idFieldType} {idFieldName.ToLower()})");
                sb.AppendLine("        {");
                sb.AppendLine($"            return rows.FirstOrDefault(r => r.{idFieldName}.Equals({idFieldName.ToLower()}));");
                sb.AppendLine("        }");
            }

            sb.AppendLine();
        }
    }
}

