using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class SkillDataRow
    {
        public int Id;
        public string Name;
        public string Element;
        public string DamageType;
        public string Shape;
        public int BaseValue;
        public float CritBonus;
        public int ManaCost;
        public int Cooldown;
        public float PreCast;
        public float CastLock;
        public float PostCast;
        public float Radius;
        public int Angle;
        public int ChainCount;
        public float KnockbackForce;
        public float IgniteBonus;
        public float SlowAmount;
        public string Notes;
    }

    [CreateAssetMenu(fileName = "SkillData", menuName = "ExcelData/SkillData")]
    public class SkillData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<SkillDataRow> rows = new List<SkillDataRow>();

        public SkillDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public SkillDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "SkillData";

    }
}