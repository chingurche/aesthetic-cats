using System;
using System.Threading;
using Core.Economy;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Core.Gameplay
{
    public class DivingSimulationService : IStartable, IDisposable
    {
        private const float BaseOxygenDrainPerSecond = 0.05f;

        private readonly PlayerModel _playerModel;
        private readonly DivingModel _divingModel;
        private readonly MainMenuModel _economyModel;
        private CancellationTokenSource _cts;

        public DivingSimulationService(
            PlayerModel playerModel,
            DivingModel divingModel,
            MainMenuModel economyModel)
        {
            _playerModel = playerModel;
            _divingModel = divingModel;
            _economyModel = economyModel;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            RunSimulationLoop(_cts.Token).Forget();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async UniTaskVoid RunSimulationLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                if (!_divingModel.IsActive || UnityEngine.Time.timeScale <= 0f)
                    continue;

                var drain = BaseOxygenDrainPerSecond * UnityEngine.Time.deltaTime / _economyModel.OxygenEfficiency;
                _playerModel.DrainOxygen(drain);
            }
        }
    }
}
