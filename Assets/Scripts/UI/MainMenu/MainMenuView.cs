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
        [SerializeField] private TextMeshProUGUI upgradeCostText;
        [SerializeField] private Button upgradeSuitButton;
        [SerializeField] private Button startGameButton;

        public event Action OnUpgradeSuitClicked;
        public event Action OnStartGameClicked;

        private void Awake()
        {
            if (upgradeSuitButton != null)
                upgradeSuitButton.onClick.AddListener(() => OnUpgradeSuitClicked?.Invoke());

            if (startGameButton != null)
                startGameButton.onClick.AddListener(() => OnStartGameClicked?.Invoke());
        }

        // Этот метод обновляет золото на экране
        public void UpdateMoneyDisplay(int money)
        {
            if (moneyText != null)
            {
                moneyText.text = $"Money: ${money}";
            }
        }

        // Этот метод обновляет стоимость апгрейда костюма
        public void UpdateUpgradeCost(int cost, int level)
        {
            if (upgradeCostText != null)
            {
                upgradeCostText.text = $"Upgrade Suit (Lvl {level}): ${cost}";
            }
        }
    }
}