using System;
using Core.Economy;
using Core.Gameplay;
using VContainer.Unity;

namespace UI.Results
{
    public class ResultsPresenter : IStartable, IDisposable
    {
        private readonly ResultsView _view;
        private readonly MainMenuModel _economyModel;
        private readonly PlayerModel _playerModel;
        private readonly DivingModel _divingModel;
        private readonly SaveSystem _saveSystem;
        private readonly UIManager _uiManager;

        private bool _rewardsProcessed;

        public ResultsPresenter(
            ResultsView view,
            MainMenuModel economyModel,
            PlayerModel playerModel,
            DivingModel divingModel,
            SaveSystem saveSystem,
            UIManager uiManager)
        {
            _view = view;
            _economyModel = economyModel;
            _playerModel = playerModel;
            _divingModel = divingModel;
            _saveSystem = saveSystem;
            _uiManager = uiManager;
        }

        public void Start()
        {
            _view.OnReturnToBaseClicked += HandleReturnToBase;
            _uiManager.OnScreenChanged += HandleScreenChanged;
        }

        private void HandleScreenChanged(UIScreen screen)
        {
            if (screen == UIScreen.Results)
                ProcessResults();
        }

        private void ProcessResults()
        {
            if (_rewardsProcessed || _divingModel.State != RunState.Finished)
                return;

            _rewardsProcessed = true;

            var moneyEarned = _divingModel.CalculateEarnings();
            _economyModel.AddEarnedMoney(moneyEarned);
            _saveSystem.SaveProgress(_economyModel);

            _view.DisplayResults(
                reason: FormatEndReason(_divingModel.EndReason),
                moneyEarned: moneyEarned,
                lootDetails: $"- Total Loot Items: {_divingModel.CurrentLoot}\n- Depth Reached: {_divingModel.CurrentDepth}m"
            );
        }

        private static string FormatEndReason(RunEndReason reason)
        {
            return reason switch
            {
                RunEndReason.OxygenDepleted => "OXYGEN DEPLETED",
                RunEndReason.HealthDepleted => "CRITICAL DAMAGE",
                RunEndReason.Surfaced => "SURFACED",
                _ => "RUN COMPLETE"
            };
        }

        private void HandleReturnToBase()
        {
            _playerModel.ResetForRun();
            _divingModel.ResetForNewRun();
            _rewardsProcessed = false;
            _uiManager.ShowMainMenu();
        }

        public void Dispose()
        {
            _view.OnReturnToBaseClicked -= HandleReturnToBase;
            _uiManager.OnScreenChanged -= HandleScreenChanged;
        }
    }
}
