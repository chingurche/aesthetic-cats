using System;
using UnityEngine;
using VContainer.Unity;

namespace Core.Gameplay
{
    public class PlayerRunSyncService : IStartable, IDisposable
    {
        private readonly PlayerModel _playerModel;
        private readonly DivingModel _divingModel;

        public PlayerRunSyncService(PlayerModel playerModel, DivingModel divingModel)
        {
            _playerModel = playerModel;
            _divingModel = divingModel;
        }

        public void Start()
        {
            _playerModel.OnDepthChanged += HandleDepthChanged;
            _playerModel.OnOxygenChanged += HandleOxygenChanged;
            _playerModel.OnOxygenDepleted += HandleOxygenDepleted;
            _playerModel.OnHealthDepleted += HandleHealthDepleted;
        }

        private void HandleDepthChanged(float depth)
        {
            if (!_divingModel.IsActive)
                return;

            _divingModel.SyncDepth(Mathf.Max(0, Mathf.RoundToInt(depth)));
        }

        private void HandleOxygenChanged(float oxygen)
        {
            if (!_divingModel.IsActive)
                return;

            _divingModel.SyncOxygen(oxygen);
        }

        private void HandleOxygenDepleted()
        {
            if (!_divingModel.IsActive)
                return;

            _divingModel.FinishRun(RunEndReason.OxygenDepleted);
        }

        private void HandleHealthDepleted()
        {
            if (!_divingModel.IsActive)
                return;

            _divingModel.FinishRun(RunEndReason.HealthDepleted);
        }

        public void Dispose()
        {
            _playerModel.OnDepthChanged -= HandleDepthChanged;
            _playerModel.OnOxygenChanged -= HandleOxygenChanged;
            _playerModel.OnOxygenDepleted -= HandleOxygenDepleted;
            _playerModel.OnHealthDepleted -= HandleHealthDepleted;
        }
    }
}
