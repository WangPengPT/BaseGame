using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class StatusEffectDataRow
    {
        public int Id;
        public string Name;
        public string Type;
        public int MaxStack;
        public float Value;
        public int Duration;
        public bool TickInterval;
        public string Notes;
    }

    [CreateAssetMenu(fileName = "StatusEffectData", menuName = "ExcelData/StatusEffectData")]
    public class StatusEffectData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<StatusEffectDataRow> rows = new List<StatusEffectDataRow>();

        public StatusEffectDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public StatusEffectDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "StatusEffectData";

    }
}