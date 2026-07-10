using System;
using Core.Economy;
using Core.Gameplay;
using UnityEngine;
using VContainer.Unity;

namespace UI.MainMenu
{
    public class MainMenuPresenter : IStartable, IDisposable
    {
        private readonly MainMenuModel _model;
        private readonly MainMenuView _view;
        private readonly PlayerModel _playerModel;
        private readonly DivingModel _divingModel;
        private readonly SaveSystem _saveSystem;
        private readonly UIManager _uiManager;

        public MainMenuPresenter(
            MainMenuModel model,
            MainMenuView view,
            PlayerModel playerModel,
            DivingModel divingModel,
            SaveSystem saveSystem,
            UIManager uiManager)
        {
            _model = model;
            _view = view;
            _playerModel = playerModel;
            _divingModel = divingModel;
            _saveSystem = saveSystem;
            _uiManager = uiManager;
        }

        public void Start()
        {
            RefreshView();

            _view.OnUpgradeSuitClicked += HandleUpgradeRequest;
            _view.OnStartGameClicked += HandleStartGame;

            _model.OnMoneyChanged += _view.UpdateMoneyDisplay;
            _model.OnSuitUpgraded += _view.UpdateUpgradeCost;
            _uiManager.OnScreenChanged += HandleScreenChanged;
        }

        private void HandleScreenChanged(UIScreen screen)
        {
            if (screen == UIScreen.MainMenu)
                RefreshView();
        }

        private void RefreshView()
        {
            _view.UpdateMoneyDisplay(_model.PlayerMoney);
            _view.UpdateUpgradeCost(_model.UpgradeCost, _model.SuitLevel);
        }

        private void HandleUpgradeRequest()
        {
            if (!_model.TryUpgradeSuit())
            {
                Debug.Log("Не хватает золота для улучшения костюма!");
                return;
            }

            _saveSystem.SaveProgress(_model);
        }

        private void HandleStartGame()
        {
            _playerModel.ResetForRun();
            _divingModel.StartRun();
            _uiManager.ShowGameplayHUD();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Dispose()
        {
            _view.OnUpgradeSuitClicked -= HandleUpgradeRequest;
            _view.OnStartGameClicked -= HandleStartGame;
            _model.OnMoneyChanged -= _view.UpdateMoneyDisplay;
            _model.OnSuitUpgraded -= _view.UpdateUpgradeCost;
            _uiManager.OnScreenChanged -= HandleScreenChanged;
        }
    }
}
