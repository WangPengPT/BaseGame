using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class WaveEntryDataRow
    {
        public int Id;
        public int WaveId;
        public int EnemyId;
        public int Count;
        public string SpawnPattern;
        public float SpawnDelay;
        public bool IsElite;
        public int AffixTags;
    }

    [CreateAssetMenu(fileName = "WaveEntryData", menuName = "ExcelData/WaveEntryData")]
    public class WaveEntryData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<WaveEntryDataRow> rows = new List<WaveEntryDataRow>();

        public WaveEntryDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public WaveEntryDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "WaveEntryData";

    }
}