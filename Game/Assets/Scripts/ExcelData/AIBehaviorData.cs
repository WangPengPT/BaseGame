using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class AIBehaviorDataRow
    {
        public int Id;
        public string Name;
        public string TargetPriority;
        public float KiteDistance;
        public float EngageRange;
        public float RetreatThresholdHp;
        public string AbilityPriority;
        public float ManaReservePct;
    }

    [CreateAssetMenu(fileName = "AIBehaviorData", menuName = "ExcelData/AIBehaviorData")]
    public class AIBehaviorData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<AIBehaviorDataRow> rows = new List<AIBehaviorDataRow>();

        public AIBehaviorDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public AIBehaviorDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "AIBehaviorData";

    }
}