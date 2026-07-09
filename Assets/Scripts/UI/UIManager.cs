using System;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Canvas Groups")]
        [SerializeField] private Canvas mainMenuCanvas;
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private Canvas pauseCanvas;
        [SerializeField] private Canvas resultsCanvas;

        public UIScreen CurrentScreen { get; private set; } = UIScreen.MainMenu;
        public event Action<UIScreen> OnScreenChanged;

        private void Awake()
        {
            SetPauseOverlay(false);
            SetScreen(UIScreen.MainMenu, invokeEvent: false);
        }

        public void SetPauseOverlay(bool visible)
        {
            if (pauseCanvas != null)
                pauseCanvas.enabled = visible;
        }

        public void ShowMainMenu()
        {
            SetScreen(UIScreen.MainMenu);
        }

        public void ShowGameplayHUD()
        {
            SetScreen(UIScreen.Gameplay);
        }

        public void ShowResults()
        {
            SetScreen(UIScreen.Results);
        }

        private void SetScreen(UIScreen screen, bool invokeEvent = true)
        {
            CurrentScreen = screen;

            if (mainMenuCanvas != null)
                mainMenuCanvas.enabled = screen == UIScreen.MainMenu;

            if (hudCanvas != null)
                hudCanvas.enabled = screen == UIScreen.Gameplay;

            if (resultsCanvas != null)
                resultsCanvas.enabled = screen == UIScreen.Results;

            if (invokeEvent)
                OnScreenChanged?.Invoke(screen);
        }
    }
}
