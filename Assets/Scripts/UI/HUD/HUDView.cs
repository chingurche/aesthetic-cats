using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class HUDView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider oxygenSlider;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI depthText;
        [SerializeField] private TextMeshProUGUI inventoryText;

        public void UpdateOxygen(float value)
        {
            if (oxygenSlider != null)
                oxygenSlider.value = value;
        }

        public void UpdateHealth(float current, float max)
        {
            if (healthSlider != null)
                healthSlider.value = max > 0f ? current / max : 0f;
        }

        public void UpdateDepth(int meters)
        {
            if (depthText != null)
                depthText.text = $"DEPTH: {meters}m";
        }

        public void UpdateInventory(int currentLoot, int maxLoot)
        {
            if (inventoryText != null)
                inventoryText.text = $"LOOT: {currentLoot} / {maxLoot}";
        }
    }
}