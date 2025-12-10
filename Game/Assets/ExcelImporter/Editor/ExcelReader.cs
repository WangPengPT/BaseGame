using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ExcelImporter
{
    public class ExcelData
    {
        public List<string> Headers { get; set; } = new List<string>();
        public List<Dictionary<string, string>> Rows { get; set; } = new List<Dictionary<string, string>>();
    }

    public class ExcelReader
    {
        public ExcelData ReadExcel(string filePath)
        {
            ExcelData data = new ExcelData();

            // 检查文件扩展名
            string extension = Path.GetExtension(filePath).ToLower();
            
            if (extension == ".csv")
            {
                return ReadCSV(filePath);
            }
            else if (extension == ".xlsx" || extension == ".xls")
            {
                // 对于 Excel 文件，我们使用简单的 CSV 转换方法
                // 注意：这需要用户先将 Excel 导出为 CSV，或者使用第三方库
                // 这里我们提供一个基础实现，建议使用 EPPlus 或 ClosedXML
                return ReadExcelAsCSV(filePath);
            }
            else
            {
                throw new NotSupportedException($"不支持的文件格式: {extension}");
            }
        }

        public ExcelData ReadCSV(string filePath)
        {
            ExcelData data = new ExcelData();
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                return data;
            }

            // 读取表头（第一行）
            data.Headers = ParseCSVLine(lines[0]).ToList();

            // 读取数据行
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string[] values = ParseCSVLine(lines[i]);
                Dictionary<string, string> row = new Dictionary<string, string>();

                for (int j = 0; j < data.Headers.Count; j++)
                {
                    string header = data.Headers[j];
                    string value = j < values.Length ? values[j] : "";
                    row[header] = value;
                }

                data.Rows.Add(row);
            }

            return data;
        }

        private ExcelData ReadExcelAsCSV(string filePath)
        {
            // 尝试使用 System.Data.OleDb 读取 Excel（需要安装 Microsoft Access Database Engine）
            // 如果失败，提示用户使用 CSV 格式
            try
            {
                return ReadExcelWithOleDb(filePath);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"无法使用 OleDb 读取 Excel: {ex.Message}");
                throw new NotSupportedException(
                    "直接读取 Excel 文件需要额外的库支持。\n" +
                    "请使用以下方法之一：\n" +
                    "1. 将 Excel 文件导出为 CSV 格式（推荐）\n" +
                    "2. 安装 Microsoft Access Database Engine\n" +
                    "3. 安装 EPPlus.Core 或 ClosedXML 库");
            }
        }

        private ExcelData ReadExcelWithOleDb(string filePath)
        {
            // 使用 System.Data.OleDb 读取 Excel
            // 注意：这需要安装 Microsoft Access Database Engine
            ExcelData data = new ExcelData();

            string connectionString = "";
            string extension = Path.GetExtension(filePath).ToLower();
            
            if (extension == ".xlsx")
            {
                connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"";
            }
            else if (extension == ".xls")
            {
                connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={filePath};Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            }
            else
            {
                throw new NotSupportedException($"不支持的文件格式: {extension}");
            }

            using (System.Data.OleDb.OleDbConnection connection = new System.Data.OleDb.OleDbConnection(connectionString))
            {
                connection.Open();
                System.Data.DataTable schemaTable = connection.GetOleDbSchemaTable(
                    System.Data.OleDb.OleDbSchemaGuid.Tables, null);
                
                if (schemaTable.Rows.Count == 0)
                {
                    throw new Exception("Excel 文件中没有工作表");
                }

                string sheetName = schemaTable.Rows[0]["TABLE_NAME"].ToString();
                string query = $"SELECT * FROM [{sheetName}]";
                
                using (System.Data.OleDb.OleDbDataAdapter adapter = new System.Data.OleDb.OleDbDataAdapter(query, connection))
                {
                    System.Data.DataSet dataSet = new System.Data.DataSet();
                    adapter.Fill(dataSet);
                    
                    System.Data.DataTable table = dataSet.Tables[0];
                    
                    if (table.Rows.Count == 0)
                    {
                        return data;
                    }

                    // 读取表头
                    foreach (System.Data.DataColumn column in table.Columns)
                    {
                        data.Headers.Add(column.ColumnName);
                    }

                    // 读取数据行
                    foreach (System.Data.DataRow row in table.Rows)
                    {
                        Dictionary<string, string> rowData = new Dictionary<string, string>();
                        foreach (string header in data.Headers)
                        {
                            rowData[header] = row[header]?.ToString() ?? "";
                        }
                        data.Rows.Add(rowData);
                    }
                }
            }

            return data;
        }

        private string[] ParseCSVLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 转义的双引号
                        currentField += '"';
                        i++;
                    }
                    else
                    {
                        // 切换引号状态
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }

            fields.Add(currentField);
            return fields.ToArray();
        }
    }
}

