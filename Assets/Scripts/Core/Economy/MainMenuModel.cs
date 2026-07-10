using System;
using UnityEngine;

namespace Core.Economy
{
    public class MainMenuModel
    {
        public int PlayerMoney { get; private set; } = 1000;
        public int SuitLevel { get; private set; } = 1;
        public int UpgradeCost => SuitLevel * 500;
        public float OxygenEfficiency => 1f + (SuitLevel - 1) * 0.25f;

        public event Action<int> OnMoneyChanged;
        public event Action<int, int> OnSuitUpgraded;

        public void LoadProgress(int money, int suitLevel)
        {
            PlayerMoney = money;
            SuitLevel = Mathf.Max(1, suitLevel);
            OnMoneyChanged?.Invoke(PlayerMoney);
            OnSuitUpgraded?.Invoke(UpgradeCost, SuitLevel);
        }

        public bool TryUpgradeSuit()
        {
            if (PlayerMoney >= UpgradeCost)
            {
                PlayerMoney -= UpgradeCost;
                SuitLevel++;
                
                OnMoneyChanged?.Invoke(PlayerMoney);
                OnSuitUpgraded?.Invoke(UpgradeCost, SuitLevel);
                return true;
            }
            return false;
        }

        // Этот метод вызовет Экран Результатов, чтобы сохранить заработанное за забег золото
        public void AddEarnedMoney(int amount)
        {
            PlayerMoney += amount;
            OnMoneyChanged?.Invoke(PlayerMoney);
        }
    }
}