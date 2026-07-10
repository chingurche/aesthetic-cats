using System;
using Core.Economy;
using Core.Gameplay;
using UnityEngine;
using VContainer.Unity;

namespace UI.HUD
{
    public class HUDPresenter : IStartable, IDisposable
    {
        private readonly PlayerModel _playerModel;
        private readonly DivingModel _divingModel;
        private readonly HUDView _view;
        private readonly UIManager _uiManager;

        public HUDPresenter(
            PlayerModel playerModel,
            DivingModel divingModel,
            HUDView view,
            UIManager uiManager)
        {
            _playerModel = playerModel;
            _divingModel = divingModel;
            _view = view;
            _uiManager = uiManager;
        }

        public void Start()
        {
            _playerModel.OnOxygenChanged += _view.UpdateOxygen;
            _playerModel.OnHealthChanged += _view.UpdateHealth;
            _playerModel.OnDepthChanged += HandleDepthChanged;
            _divingModel.OnInventoryChanged += _view.UpdateInventory;
            _divingModel.OnRunEnded += HandleRunEnded;
            _uiManager.OnScreenChanged += HandleScreenChanged;
        }

        private void HandleScreenChanged(UIScreen screen)
        {
            if (screen == UIScreen.Gameplay)
                RefreshView();
        }

        private void HandleDepthChanged(float depth)
        {
            _view.UpdateDepth(Mathf.RoundToInt(depth));
        }

        private void RefreshView()
        {
            _view.UpdateOxygen(_playerModel.Oxygen);
            _view.UpdateHealth(_playerModel.Health, _playerModel.MaxHealth);
            _view.UpdateDepth(Mathf.RoundToInt(_playerModel.Depth));
            _view.UpdateInventory(_divingModel.CurrentLoot, _divingModel.MaxLoot);
        }

        private void HandleRunEnded(RunEndReason reason)
        {
            _uiManager.ShowResults();
        }

        public void Dispose()
        {
            _playerModel.OnOxygenChanged -= _view.UpdateOxygen;
            _playerModel.OnHealthChanged -= _view.UpdateHealth;
            _playerModel.OnDepthChanged -= HandleDepthChanged;
            _divingModel.OnInventoryChanged -= _view.UpdateInventory;
            _divingModel.OnRunEnded -= HandleRunEnded;
            _uiManager.OnScreenChanged -= HandleScreenChanged;
        }
    }
}
