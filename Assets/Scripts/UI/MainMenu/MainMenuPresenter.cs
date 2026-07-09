using System;
using UnityEngine;
using VContainer.Unity;
using Core.Economy;
using Core.Gameplay;

namespace UI.MainMenu
{
    public class MainMenuPresenter : IStartable, IDisposable
    {
        private readonly MainMenuModel _model;
        private readonly MainMenuView _view;
        private readonly DivingModel _divingModel;
        private readonly UIManager _uiManager;

        public MainMenuPresenter(
            MainMenuModel model,
            MainMenuView view,
            DivingModel divingModel,
            UIManager uiManager)
        {
            _model = model;
            _view = view;
            _divingModel = divingModel;
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
                Debug.Log("Не хватает золота для улучшения костюма!");
        }

        private void HandleStartGame()
        {
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
