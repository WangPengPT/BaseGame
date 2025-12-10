using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// UI 自定义显示方法集合
    /// 在这里添加自定义的显示方法，供 DataBinder 使用
    /// </summary>
    public static class UIFunctions
    {
        /// <summary>
        /// 示例：自定义显示整数为带格式的文本
        /// </summary>
        public static void DisplayIntWithFormat(GameObject target, int value)
        {
            Text text = target.GetComponent<Text>();
            if (text != null)
            {
                text.text = $"数值: {value:N0}";
            }
        }

        /// <summary>
        /// 示例：自定义显示浮点数为百分比
        /// </summary>
        public static void DisplayFloatAsPercent(GameObject target, float value)
        {
            Text text = target.GetComponent<Text>();
            if (text != null)
            {
                text.text = $"{value * 100:F1}%";
            }
        }

        /// <summary>
        /// 示例：自定义显示布尔值为中文
        /// </summary>
        public static void DisplayBoolAsChinese(GameObject target, bool value)
        {
            Text text = target.GetComponent<Text>();
            if (text != null)
            {
                text.text = value ? "是" : "否";
            }
        }

        // 在这里添加更多自定义显示方法...
        // 方法签名必须是: public static void MethodName(GameObject target, T value)
        // 其中 T 是字段的数据类型
    }
}
