using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Results
{
    public class ResultsView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI reasonText;
        [SerializeField] private TextMeshProUGUI totalEarningsText;
        [SerializeField] private TextMeshProUGUI lootSummaryText;
        [SerializeField] private Button toFullMenuButton;

        // Событие клика на кнопку, которое заберет Презентер
        public event Action OnReturnToBaseClicked;

        private void Awake()
        {
            if (toFullMenuButton != null)
                toFullMenuButton.onClick.AddListener(() => OnReturnToBaseClicked?.Invoke());
        }

        // Метод для заполнения результатов текстом
        public void DisplayResults(string reason, int moneyEarned, string lootDetails)
        {
            if (reasonText != null)
                reasonText.text = reason;

            if (totalEarningsText != null)
                totalEarningsText.text = $"Earned: {moneyEarned}$";

            if (lootSummaryText != null)
                lootSummaryText.text = $"Collected:\n{lootDetails}";
        }
    }
}