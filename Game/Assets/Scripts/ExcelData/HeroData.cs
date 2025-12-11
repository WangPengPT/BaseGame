using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class HeroDataRow
    {
        public int Id;
        public string Name;
        public string Role;
        public string PrimaryElement;
        public int BaseHp;
        public int BaseMp;
        public int Armor;
        public float PhysicalResist;
        public float MagicalResist;
        public float IceResist;
        public float FireResist;
        public float LightningResist;
        public float PoisonResist;
        public float CritChance;
        public float CritMultiplier;
        public float Dodge;
        public float AttackSpeed;
        public string BaseSkills;
        public bool AiBehaviorId;
        public int ResistProfileId;
    }

    [CreateAssetMenu(fileName = "HeroData", menuName = "ExcelData/HeroData")]
    public class HeroData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<HeroDataRow> rows = new List<HeroDataRow>();

        public HeroDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public HeroDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "HeroData";

    }
}