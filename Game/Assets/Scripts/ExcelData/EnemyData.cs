using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class EnemyDataRow
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
        public int BaseSkills;
        public int AiBehaviorId;
        public int ResistProfileId;
        public string PrefabPath;
    }

    [CreateAssetMenu(fileName = "EnemyData", menuName = "ExcelData/EnemyData")]
    public class EnemyData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<EnemyDataRow> rows = new List<EnemyDataRow>();

        public EnemyDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public EnemyDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "EnemyData";

    }
}