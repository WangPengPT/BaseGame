using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class LegendaryDataRow
    {
        public int Id;
        public string Name;
        public string Slot;
        public string UniqueEffectDescription;
        public string EffectHook;
        public string Values;
        public string Downside;
    }

    [CreateAssetMenu(fileName = "LegendaryData", menuName = "ExcelData/LegendaryData")]
    public class LegendaryData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<LegendaryDataRow> rows = new List<LegendaryDataRow>();

        public LegendaryDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public LegendaryDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "LegendaryData";

    }
}