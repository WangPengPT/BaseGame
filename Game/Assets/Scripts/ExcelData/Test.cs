using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExcelData
{

    [Serializable]
    public class TestRow
    {
        public int Id;
        public string Name;
        public int Age;
        public float Score;
        public bool Isactive;
    }

    [CreateAssetMenu(fileName = "Test", menuName = "ExcelData/Test")]
    public class Test : ScriptableObject, ExcelImporter.IExcelDataTable
    {
        public List<TestRow> rows = new List<TestRow>();

        public TestRow GetRow(int index)
        {
            if (index >= 0 && index < rows.Count)
                return rows[index];
            return null;
        }

        /// <summary>
        /// 根据 Id 查找数据行
        /// </summary>
        public TestRow GetById(int id)
        {
            return rows.FirstOrDefault(r => r.Id == id);
        }

        public int Count => rows.Count;

        public string TableName => "Test";

    }
}