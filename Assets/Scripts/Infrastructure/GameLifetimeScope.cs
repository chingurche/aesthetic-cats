using UnityEngine;
using VContainer;
using VContainer.Unity;
using UI.MainMenu;
using UI.HUD; 

namespace Infrastructure
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("UI Views")]
        [SerializeField] private MainMenuView mainMenuView;
        [SerializeField] private HUDView hudView; 

        protected override void Configure(IContainerBuilder builder)
        {
            // --- РЕГИСТРАЦИЯ ГЛАВНОГО МЕНЮ ---
            builder.RegisterComponent(mainMenuView);
            builder.RegisterEntryPoint<MainMenuPresenter>();

            // --- РЕГИСТРАЦИЯ ИГРОВОГО HUD ---
            builder.RegisterComponent(hudView);
            
            builder.RegisterEntryPoint<HUDPresenter>();
        }
    }
}