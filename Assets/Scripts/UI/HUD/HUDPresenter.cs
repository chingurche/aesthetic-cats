using System;
using UnityEngine;
using VContainer.Unity;

namespace UI.HUD
{
    public class HUDPresenter : IStartable, IDisposable
    {
        private readonly HUDView _view;

        public HUDPresenter(HUDView view)
        {
            _view = view;
        }

        public void Start()
        {
            Debug.Log("HUDPresenter успешно запущен! Надо логику погружения.");
            
            _view.UpdateOxygen(1.0f); // Полный баллон (100%)
            _view.UpdateDepth(0);     // Глубина 0 метров
            _view.UpdateInventory(0, 10); // Лута в рюкзаке 0 из 10
        }

        public void Dispose()
        {
            // Здесь мы в будущем будем отписываться от событий Модели игрока
        }
    }
}