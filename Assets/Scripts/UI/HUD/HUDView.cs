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
            oxygenSlider.value = value;
        }

        // Метод для обновления метров глубины
        public void UpdateDepth(int meters)
        {
            depthText.text = $"DEPTH: {meters}m";
        }

        // Метод для обновления счетчика рюкзака
        public void UpdateInventory(int currentLoot, int maxLoot)
        {
            inventoryText.text = $"LOOT: {currentLoot} / {maxLoot}";
        }
    }
}