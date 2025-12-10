using UnityEngine;
using UI;

namespace UI.Examples
{
    /// <summary>
    /// DataBinder 使用示例
    /// </summary>
    public class DataBinderExample : MonoBehaviour
    {
        [Header("测试数据")]
        public GameObject uiPanel;

        private void Start()
        {
            // 示例：创建一个测试数据对象
            // 假设在 Datas 命名空间下有一个 PlayerData 类
            /*
            Datas.PlayerData playerData = new Datas.PlayerData
            {
                Name = "张三",
                Level = 10,
                IsVip = true,
                Exp = 0.75f
            };

            // Update UI
            UITools.Update(uiPanel, playerData);
            */
        }

        private void Update()
        {
            // Example: Press space to update UI
            if (Input.GetKeyDown(KeyCode.Space) && uiPanel != null)
            {
                // UITools.Update(uiPanel, someData);
            }
        }
    }
}

