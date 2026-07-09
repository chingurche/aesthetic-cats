using UnityEngine;
using VContainer;
using VContainer.Unity;
using Core.Economy;
using Core.Gameplay;
using UI;
using UI.MainMenu;
using UI.HUD;
using UI.Results;
using UI.Pause;

namespace Infrastructure
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Managers")]
        [SerializeField] private UIManager uiManager;

        [Header("UI Views")]
        [SerializeField] private MainMenuView mainMenuView;
        [SerializeField] private HUDView hudView;
        [SerializeField] private PauseView pauseView;
        [SerializeField] private ResultsView resultsView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(uiManager);

            builder.Register<MainMenuModel>(Lifetime.Singleton);
            builder.Register<DivingModel>(Lifetime.Singleton);
            builder.Register<DivingGameplayBridge>(Lifetime.Singleton);
            builder.Register<GamePauseModel>(Lifetime.Singleton);

            builder.RegisterEntryPoint<DivingSimulationService>();

            builder.RegisterComponent(mainMenuView);
            builder.RegisterEntryPoint<MainMenuPresenter>();

            builder.RegisterComponent(hudView);
            builder.RegisterEntryPoint<HUDPresenter>();

            builder.RegisterComponent(pauseView);
            builder.RegisterEntryPoint<PausePresenter>();

            builder.RegisterComponent(resultsView);
            builder.RegisterEntryPoint<ResultsPresenter>();
        }
    }
}
