using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ExcelImporter
{
    /// <summary>
    /// 使用 EPPlus 读取 Excel 文件的实现
    /// 注意：需要安装 EPPlus.Core NuGet 包或 DLL
    /// </summary>
    public class ExcelReaderEPPlus
    {
        public ExcelData ReadExcel(string filePath)
        {
            ExcelData data = new ExcelData();

            try
            {
                // 尝试使用 EPPlus
                // 注意：这需要 EPPlus.Core 库
                // 如果未安装，将回退到 CSV 读取
                
                // 检查文件是否存在
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"文件不存在: {filePath}");
                }

                // 这里使用反射来尝试加载 EPPlus
                // 如果 EPPlus 未安装，将抛出异常并回退到 CSV
                return ReadExcelWithEPPlus(filePath);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"无法使用 EPPlus 读取 Excel，尝试使用 CSV 方式: {ex.Message}");
                
                // 回退到 CSV 读取
                ExcelReader csvReader = new ExcelReader();
                return csvReader.ReadCSV(filePath);
            }
        }

        private ExcelData ReadExcelWithEPPlus(string filePath)
        {
            ExcelData data = new ExcelData();

            // 尝试通过反射使用 EPPlus
            // 这需要 EPPlus.Core 库
            var assembly = System.Reflection.Assembly.Load("EPPlus");
            if (assembly == null)
            {
                throw new Exception("EPPlus 库未找到。请安装 EPPlus.Core 或使用 CSV 格式。");
            }

            var packageType = assembly.GetType("OfficeOpenXml.ExcelPackage");
            var worksheetType = assembly.GetType("OfficeOpenXml.ExcelWorksheet");
            
            if (packageType == null || worksheetType == null)
            {
                throw new Exception("EPPlus 类型未找到");
            }

            // 创建 ExcelPackage 实例
            var package = Activator.CreateInstance(packageType, new FileInfo(filePath));
            var workbookProperty = packageType.GetProperty("Workbook");
            var workbook = workbookProperty.GetValue(package);
            
            var worksheetsProperty = workbook.GetType().GetProperty("Worksheets");
            var worksheets = worksheetsProperty.GetValue(workbook) as System.Collections.IEnumerable;
            
            // 获取第一个工作表
            System.Collections.IEnumerator enumerator = worksheets.GetEnumerator();
            enumerator.MoveNext();
            var worksheet = enumerator.Current;
            
            // 读取数据
            var dimensionProperty = worksheetType.GetProperty("Dimension");
            var dimension = dimensionProperty.GetValue(worksheet);
            
            if (dimension == null)
            {
                return data;
            }

            var rowsProperty = dimension.GetType().GetProperty("Rows");
            var columnsProperty = dimension.GetType().GetProperty("Columns");
            var endRowProperty = dimension.GetType().GetProperty("End");
            var endRow = endRowProperty.GetValue(dimension);
            
            var rowProperty = endRow.GetType().GetProperty("Row");
            var columnProperty = endRow.GetType().GetProperty("Column");
            
            int rowCount = (int)rowProperty.GetValue(endRow);
            int colCount = (int)columnProperty.GetValue(endRow);

            var cellsProperty = worksheetType.GetProperty("Cells");
            var cells = cellsProperty.GetValue(worksheet);

            // 读取表头
            for (int col = 1; col <= colCount; col++)
            {
                var cellMethod = cells.GetType().GetMethod("get_Item", new[] { typeof(int), typeof(int) });
                var cell = cellMethod.Invoke(cells, new object[] { 1, col });
                var valueProperty = cell.GetType().GetProperty("Value");
                var value = valueProperty.GetValue(cell);
                data.Headers.Add(value?.ToString() ?? $"Column{col}");
            }

            // 读取数据行
            for (int row = 2; row <= rowCount; row++)
            {
                Dictionary<string, string> rowData = new Dictionary<string, string>();
                
                for (int col = 1; col <= colCount; col++)
                {
                    var cellMethod = cells.GetType().GetMethod("get_Item", new[] { typeof(int), typeof(int) });
                    var cell = cellMethod.Invoke(cells, new object[] { row, col });
                    var valueProperty = cell.GetType().GetProperty("Value");
                    var value = valueProperty.GetValue(cell);
                    
                    string header = col <= data.Headers.Count ? data.Headers[col - 1] : $"Column{col}";
                    rowData[header] = value?.ToString() ?? "";
                }
                
                data.Rows.Add(rowData);
            }

            // 释放资源
            var disposeMethod = packageType.GetMethod("Dispose");
            disposeMethod.Invoke(package, null);

            return data;
        }
    }
}

