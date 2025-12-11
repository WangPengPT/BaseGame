using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class WaveRewardDataRow
    {
        public int Id;
        public string RewardTableId;
        public float RarityBias;
        public int GoldMin;
        public int GoldMax;
        public int MaterialPool;
        public int ConsumablePool;
        public string ItemPoolId;
        public float ScoreToLootScalar;
    }

    [CreateAssetMenu(fileName = "WaveRewardData", menuName = "ExcelData/WaveRewardData")]
    public class WaveRewardData : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<WaveRewardDataRow> rows = new List<WaveRewardDataRow>();

        public WaveRewardDataRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public WaveRewardDataRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "WaveRewardData";

    }
}