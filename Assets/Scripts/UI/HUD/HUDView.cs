using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class HUDView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider oxygenSlider;
        [SerializeField] private TextMeshProUGUI depthText;
        [SerializeField] private TextMeshProUGUI inventoryText;

        // Метод, который Презентер будет вызывать каждую секунду для полоски воздуха
        // value будет приходить от 0.0 (пусто) до 1.0 (полный баллон)
        public void UpdateOxygen(float value)
        {
            if (oxygenSlider != null)
                oxygenSlider.value = value;
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