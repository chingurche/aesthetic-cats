using System;
using UnityEngine;
using VContainer.Unity;

namespace UI.MainMenu
{
    public class MainMenuPresenter : IStartable, IDisposable
    {
        private readonly MainMenuView _view;

        // VContainer сам прокинет сюда вьюшку, которую мы зарегистрируем в коде ниже
        public MainMenuPresenter(MainMenuView view)
        {
            _view = view;
        }

        public void Start()
        {
            // Подписываемся на события кнопок из View
            _view.OnUpgradeSuitClicked += HandleUpgrade;
            _view.OnStartGameClicked += HandleStartGame;
            
            Debug.Log("Presenter Главного меню успешно запущен!");
        }

        private void HandleUpgrade()
        {
            Debug.Log("Кликнули апгрейд! Тут будет списывание денег через Модель.");
        }

        private void HandleStartGame()
        {
            Debug.Log("Погружение началось! Запускаем генерацию карты...");
        }

        public void Dispose()
        {
            _view.OnUpgradeSuitClicked -= HandleUpgrade;
            _view.OnStartGameClicked -= HandleStartGame;
        }
    }
}