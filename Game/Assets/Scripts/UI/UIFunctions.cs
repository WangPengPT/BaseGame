using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// UI custom display methods collection
    /// Add custom display methods here for DataBinder to use
    /// </summary>
    public static class UIFunctions
    {
        /// <summary>
        /// Example: Custom display integer as formatted text
        /// </summary>
        public static void DisplayIntWithFormat(GameObject target, int value)
        {
            Text text = target.GetComponent<Text>();
            if (text != null)
            {
                text.text = $"Value: {value:N0}";
            }
        }

        /// <summary>
        /// Example: Custom display float as percentage
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
        /// Example: Custom display boolean as text
        /// </summary>
        public static void DisplayBoolAsText(GameObject target, bool value)
        {
            Text text = target.GetComponent<Text>();
            if (text != null)
            {
                text.text = value ? "Yes" : "No";
            }
        }

        // Add more custom display methods here...
        // Method signature must be: public static void MethodName(GameObject target, T value)
        // Where T is the data type of the field
    }
}
