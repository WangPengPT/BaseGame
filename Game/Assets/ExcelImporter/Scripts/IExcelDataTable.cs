using UnityEngine;

namespace ExcelImporter
{
    /// <summary>
    /// Excel 数据表接口，所有生成的 Excel 数据类都应实现此接口
    /// </summary>
    public interface IExcelDataTable
    {
        /// <summary>
        /// 获取数据行数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取表名（类名）
        /// </summary>
        string TableName { get; }
    }
}

