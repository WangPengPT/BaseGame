using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class ResistProfileDataRow
    {
        public int Id;
        public float IceResist;
        public float FireResist;
        public float LightningResist;
        public float PoisonResist;
        public string ImmunityFlags;
    }

    [CreateAssetMenu(fileName = "ResistProfileData", menuName = "ExcelData/ResistProfileData")]
    public class ResistProfileData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<ResistProfileDataRow> rows = new List<ResistProfileDataRow>();

        public ResistProfileDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public ResistProfileDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "ResistProfileData";

    }
}