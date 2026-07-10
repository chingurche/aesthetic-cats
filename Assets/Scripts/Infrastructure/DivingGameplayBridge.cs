using Core.Gameplay;
using VContainer;

namespace Infrastructure
{
    public class DivingGameplayBridge
    {
        private readonly DivingModel _model;

        [Inject]
        public DivingGameplayBridge(DivingModel model)
        {
            _model = model;
        }

        public bool IsRunActive => _model.IsActive;

        public void AddDepth(int meters) => _model.SyncDepth(_model.CurrentDepth + meters);

        public void AddLoot(int amount) => _model.AddLoot(amount);

        public void Surface() => _model.FinishRun(RunEndReason.Surfaced);
    }
}
