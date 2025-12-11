using System;
using System.Collections.Generic;
using System.Linq;
using ExcelImporter;
using UnityEngine;

namespace Game.Combat.Data
{
    [Serializable]
    public class AbilityDataRow
    {
        public string Id;
        public string Name;
        public string Element;
        public string Damage_Type;
        public string Shape;
        public float Base_Value;
        public float Crit_Bonus;
        public float Mana_Cost;
        public float Cooldown;
        public float Pre_Cast;
        public float Cast_Lock;
        public float Post_Cast;
        public float Radius;
        public float Angle;
        public int Max_Targets;
        public int Chain_Count;
        public float Knockback_Force;
        public float Ignite_Bonus;
        public float Slow_Amount;
    }

    [CreateAssetMenu(fileName = "AbilityData", menuName = "ExcelData/AbilityData")]
    public class AbilityData : ScriptableObject, IExcelDataTable
    {
        public List<AbilityDataRow> rows = new List<AbilityDataRow>();

        public AbilityDataRow GetById(string id) => rows.FirstOrDefault(r => r.Id == id);

        public int Count => rows.Count;
        public string TableName => "AbilityData";
    }
}

