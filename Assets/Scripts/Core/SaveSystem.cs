using Core.Economy;
using UnityEngine;

public class SaveSystem
{
    private const string MoneyKey = "player_money";
    private const string SuitLevelKey = "suit_level";

    public void Initialize(MainMenuModel economyModel, PlayerModel playerModel)
    {
        if (!PlayerPrefs.HasKey(MoneyKey))
            return;

        economyModel.LoadProgress(
            PlayerPrefs.GetInt(MoneyKey, economyModel.PlayerMoney),
            PlayerPrefs.GetInt(SuitLevelKey, economyModel.SuitLevel));

        playerModel.ResetForRun();
    }

    public void SaveProgress(MainMenuModel economyModel)
    {
        PlayerPrefs.SetInt(MoneyKey, economyModel.PlayerMoney);
        PlayerPrefs.SetInt(SuitLevelKey, economyModel.SuitLevel);
        PlayerPrefs.Save();
    }
}
