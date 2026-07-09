using System;

namespace Core.Gameplay
{
    public class GamePauseModel
    {
        public bool IsPaused { get; private set; }

        public event Action<bool> OnPauseChanged;

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused)
                return;

            IsPaused = paused;
            OnPauseChanged?.Invoke(paused);
        }

        public void Toggle()
        {
            SetPaused(!IsPaused);
        }
    }
}
