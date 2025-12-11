using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class AffixDataRow
    {
        public int Id;
        public string Name;
        public string Category;
        public string ElementTag;
        public int MinValue;
        public int MaxValue;
        public string RollType;
        public string Target;
        public int MaxRollsPerItem;
        public string Tags;
    }

    [CreateAssetMenu(fileName = "AffixData", menuName = "ExcelData/AffixData")]
    public class AffixData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<AffixDataRow> rows = new List<AffixDataRow>();

        public AffixDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public AffixDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "AffixData";

    }
}