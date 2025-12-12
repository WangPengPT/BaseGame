using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class SetDataRow
    {
        public int SetId;
        public string Name;
        public int PieceIds;
    }

    [CreateAssetMenu(fileName = "SetData", menuName = "ExcelData/SetData")]
    public class SetData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<SetDataRow> rows = new List<SetDataRow>();

        public SetDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        public int Count => rows.Count;

        public string TableName => "SetData";

    }
}