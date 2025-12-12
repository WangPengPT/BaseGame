using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class LootTableDataRow
    {
        public int TableId;
        public int EntryId;
        public int ItemId;
        public int PoolId;
        public int Weight;
        public int MinLevel;
        public int MaxLevel;
        public string QuantityRange;
        public float RarityBias;
    }

    [CreateAssetMenu(fileName = "LootTableData", menuName = "ExcelData/LootTableData")]
    public class LootTableData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<LootTableDataRow> rows = new List<LootTableDataRow>();

        public LootTableDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        public int Count => rows.Count;

        public string TableName => "LootTableData";

    }
}