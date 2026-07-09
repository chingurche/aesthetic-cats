using System;
using Core.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace UI.Pause
{
    public class PausePresenter : IStartable, ITickable, IDisposable
    {
        private readonly GamePauseModel _pauseModel;
        private readonly DivingModel _divingModel;
        private readonly PauseView _view;
        private readonly UIManager _uiManager;

        public PausePresenter(
            GamePauseModel pauseModel,
            DivingModel divingModel,
            PauseView view,
            UIManager uiManager)
        {
            _pauseModel = pauseModel;
            _divingModel = divingModel;
            _view = view;
            _uiManager = uiManager;
        }

        public void Start()
        {
            _view.OnResumeClicked += HandleResume;
            _view.OnQuitToMenuClicked += HandleQuitToMenu;
            _pauseModel.OnPauseChanged += HandlePauseChanged;
            _uiManager.OnScreenChanged += HandleScreenChanged;
        }

        public void Tick()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
                return;

            if (_uiManager.CurrentScreen != UIScreen.Gameplay)
                return;

            _pauseModel.Toggle();
        }

        private void HandlePauseChanged(bool isPaused)
        {
            _uiManager.SetPauseOverlay(isPaused);
            Time.timeScale = isPaused ? 0f : 1f;
            ApplyCursor(isPaused);
        }

        private void HandleScreenChanged(UIScreen screen)
        {
            if (screen == UIScreen.Gameplay)
                return;

            if (_pauseModel.IsPaused)
                _pauseModel.SetPaused(false);
        }

        private void HandleResume()
        {
            _pauseModel.SetPaused(false);
        }

        private void HandleQuitToMenu()
        {
            _pauseModel.SetPaused(false);
            _divingModel.ResetForNewRun();
            _uiManager.ShowMainMenu();
            ApplyCursor(true);
        }

        private static void ApplyCursor(bool menuVisible)
        {
            Cursor.lockState = menuVisible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = menuVisible;
        }

        public void Dispose()
        {
            _view.OnResumeClicked -= HandleResume;
            _view.OnQuitToMenuClicked -= HandleQuitToMenu;
            _pauseModel.OnPauseChanged -= HandlePauseChanged;
            _uiManager.OnScreenChanged -= HandleScreenChanged;

            if (_pauseModel.IsPaused)
            {
                _pauseModel.SetPaused(false);
                Time.timeScale = 1f;
            }
        }
    }
}
