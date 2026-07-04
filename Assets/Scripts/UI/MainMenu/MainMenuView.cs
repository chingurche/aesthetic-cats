using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private Button upgradeSuitButton;
        [SerializeField] private TextMeshProUGUI upgradeButtonText;
        [SerializeField] private Button startGameButton;
        
        
        public event Action OnUpgradeSuitClicked;
        public event Action OnStartGameClicked;

        private void Awake()
        {
            upgradeSuitButton.onClick.AddListener(() => OnUpgradeSuitClicked?.Invoke());
            startGameButton.onClick.AddListener(() => OnStartGameClicked?.Invoke());
        }
        
        public void UpdateMoneyDisply (int currentMoney)
        {
            moneyText.text = $"Денюжки: {currentMoney}";
        }

        public void UpdateUpgradeCost(int cost, int currentLevel)
        {
            upgradeButtonText.text = $"Улучшение одежды до Lv.{currentLevel + 1} ({cost})";
        }
    }
}