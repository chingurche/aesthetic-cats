using System;
using VContainer.Unity;
using Core.Gameplay;

namespace UI.HUD
{
    public class HUDPresenter : IStartable, IDisposable
    {
        private readonly DivingModel _divingModel;
        private readonly HUDView _view;
        private readonly UIManager _uiManager;

        public HUDPresenter(DivingModel divingModel, HUDView view, UIManager uiManager)
        {
            _divingModel = divingModel;
            _view = view;
            _uiManager = uiManager;
        }

        public void Start()
        {
            _divingModel.OnOxygenChanged += _view.UpdateOxygen;
            _divingModel.OnDepthChanged += _view.UpdateDepth;
            _divingModel.OnInventoryChanged += _view.UpdateInventory;
            _divingModel.OnRunEnded += HandleRunEnded;
            _uiManager.OnScreenChanged += HandleScreenChanged;
        }

        private void HandleScreenChanged(UIScreen screen)
        {
            if (screen == UIScreen.Gameplay)
                RefreshView();
        }

        private void RefreshView()
        {
            _view.UpdateOxygen(_divingModel.CurrentOxygen);
            _view.UpdateDepth(_divingModel.CurrentDepth);
            _view.UpdateInventory(_divingModel.CurrentLoot, _divingModel.MaxLoot);
        }

        private void HandleRunEnded(RunEndReason reason)
        {
            _uiManager.ShowResults();
        }

        public void Dispose()
        {
            _divingModel.OnOxygenChanged -= _view.UpdateOxygen;
            _divingModel.OnDepthChanged -= _view.UpdateDepth;
            _divingModel.OnInventoryChanged -= _view.UpdateInventory;
            _divingModel.OnRunEnded -= HandleRunEnded;
            _uiManager.OnScreenChanged -= HandleScreenChanged;
        }
    }
}
