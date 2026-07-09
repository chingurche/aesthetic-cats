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

        public void Tick(float oxygenDrain)
        {
            if (!IsActive)
                return;

            CurrentOxygen = Math.Max(0f, CurrentOxygen - oxygenDrain);
            OnOxygenChanged?.Invoke(CurrentOxygen);

            if (CurrentOxygen <= 0f)
                FinishRun(RunEndReason.OxygenDepleted);
        }

        public void AddDepth(int meters)
        {
            if (!IsActive || meters <= 0)
                return;

            CurrentDepth += meters;
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
