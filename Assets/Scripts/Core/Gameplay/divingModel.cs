using System;

namespace Core.Gameplay
{
    public enum RunState
    {
        Idle,
        InProgress,
        Finished
    }

    public class DivingModel
    {
        public float CurrentOxygen { get; private set; } = 1f;
        public int CurrentDepth { get; private set; }
        public int CurrentLoot { get; private set; }
        public int MaxLoot { get; private set; } = 10;
        public RunState State { get; private set; } = RunState.Idle;
        public RunEndReason EndReason { get; private set; } = RunEndReason.None;

        public bool IsActive => State == RunState.InProgress;

        public event Action<float> OnOxygenChanged;
        public event Action<int> OnDepthChanged;
        public event Action<int, int> OnInventoryChanged;
        public event Action<RunEndReason> OnRunEnded;

        public void StartRun()
        {
            ResetStats();
            State = RunState.InProgress;
            EndReason = RunEndReason.None;
            NotifyAll();
        }

        public void SyncOxygen(float value)
        {
            CurrentOxygen = Math.Clamp(value, 0f, 1f);
            OnOxygenChanged?.Invoke(CurrentOxygen);
        }

        public void SyncDepth(int meters)
        {
            CurrentDepth = Math.Max(0, meters);
            OnDepthChanged?.Invoke(CurrentDepth);
        }

        public void AddLoot(int amount)
        {
            if (!IsActive || amount <= 0)
                return;

            if (CurrentLoot + amount > MaxLoot)
                return;

            CurrentLoot += amount;
            OnInventoryChanged?.Invoke(CurrentLoot, MaxLoot);
        }

        public void FinishRun(RunEndReason reason)
        {
            if (!IsActive)
                return;

            State = RunState.Finished;
            EndReason = reason;
            OnRunEnded?.Invoke(reason);
        }

        public int CalculateEarnings()
        {
            return CurrentDepth * 2 + CurrentLoot * 100;
        }

        public void ResetForNewRun()
        {
            ResetStats();
            State = RunState.Idle;
            EndReason = RunEndReason.None;
        }

        private void ResetStats()
        {
            CurrentOxygen = 1f;
            CurrentDepth = 0;
            CurrentLoot = 0;
        }

        private void NotifyAll()
        {
            OnOxygenChanged?.Invoke(CurrentOxygen);
            OnDepthChanged?.Invoke(CurrentDepth);
            OnInventoryChanged?.Invoke(CurrentLoot, MaxLoot);
        }
    }
}
