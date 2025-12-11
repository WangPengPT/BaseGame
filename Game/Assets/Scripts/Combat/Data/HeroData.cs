using System;
using System.Collections.Generic;
using System.Linq;
using ExcelImporter;
using UnityEngine;

namespace Game.Combat.Data
{
    [Serializable]
    public class HeroDataRow
    {
        public int Id;
        public string Name;
        public string Role;
        public string Primary_Element;
        public float Base_Hp;
        public float Base_Mp;
        public float Armor;
        public float Physical_Resist;
        public float Magical_Resist;
        public float Ice_Resist;
        public float Fire_Resist;
        public float Lightning_Resist;
        public float Poison_Resist;
        public float Crit_Chance;
        public float Crit_Multiplier;
        public float Dodge;
        public float Attack_Speed;
        public string Base_Skills;
        public int Ai_Behavior_Id;
        public int Resist_Profile_Id;
    }

    [CreateAssetMenu(fileName = "HeroData", menuName = "ExcelData/HeroData")]
    public class HeroData : ScriptableObject, IExcelDataTable
    {
        public List<HeroDataRow> rows = new List<HeroDataRow>();

        public HeroDataRow GetById(int id) => rows.FirstOrDefault(r => r.Id == id);
        public int Count => rows.Count;
        public string TableName => "HeroData";
    }
}

