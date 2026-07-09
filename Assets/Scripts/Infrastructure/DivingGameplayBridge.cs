using Core.Gameplay;
using VContainer;

namespace Infrastructure
{
    /// <summary>
    /// Точка входа для игрового кода: глубина, лут и завершение забега.
    /// Инжектится через VContainer в MonoBehaviour-ы сцены.
    /// </summary>
    public class DivingGameplayBridge
    {
        private readonly DivingModel _model;

        [Inject]
        public DivingGameplayBridge(DivingModel model)
        {
            _model = model;
        }

        public bool IsRunActive => _model.IsActive;

        public void AddDepth(int meters) => _model.AddDepth(meters);

        public void AddLoot(int amount) => _model.AddLoot(amount);

        public void Surface() => _model.FinishRun(RunEndReason.Surfaced);
    }
}
