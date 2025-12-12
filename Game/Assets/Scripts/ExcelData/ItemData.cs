using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class ItemDataRow
    {
        public int Id;
        public string Name;
        public string Slot;
        public string Rarity;
        public int RequiredLevel;
        public string BaseStat;
        public string AllowedAffixPoolIds;
        public int LegendaryId;
        public int SetId;
    }

    [CreateAssetMenu(fileName = "ItemData", menuName = "ExcelData/ItemData")]
    public class ItemData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<ItemDataRow> rows = new List<ItemDataRow>();

        public ItemDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public ItemDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "ItemData";

    }
}