using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class SetBonusDataRow
    {
        public int SetId;
        public int PiecesRequired;
        public string BonusDescription;
        public string EffectHook;
        public string Values;
    }

    [CreateAssetMenu(fileName = "SetBonusData", menuName = "ExcelData/SetBonusData")]
    public class SetBonusData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<SetBonusDataRow> rows = new List<SetBonusDataRow>();

        public SetBonusDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        public int Count => rows.Count;

        public string TableName => "SetBonusData";

    }
}