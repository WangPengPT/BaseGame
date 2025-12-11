using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class WaveDataRow
    {
        public int Id;
        public string Name;
        public int WaveIndex;
        public bool IsBossWave;
        public float DifficultyScalar;
        public float PrepTime;
        public bool AutoStart;
        public float EarlyStartBonus;
        public string RewardTableId;
        public string SpawnPointGroupName;
        public string CenterPointName;
        public string EnemyPrefabPath;
    }

    [CreateAssetMenu(fileName = "WaveData", menuName = "ExcelData/WaveData")]
    public class WaveData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<WaveDataRow> rows = new List<WaveDataRow>();

        public WaveDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public WaveDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "WaveData";

    }
}