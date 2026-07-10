using Core.Economy;
using Core.Gameplay;
using Infrastructure;
using UI;
using UI.HUD;
using UI.MainMenu;
using UI.Pause;
using UI.Results;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<AudioSystem>(Lifetime.Singleton);
        builder.Register<SaveSystem>(Lifetime.Singleton);

        builder.Register<PlayerModel>(Lifetime.Singleton);
        builder.Register<MainMenuModel>(Lifetime.Singleton);
        builder.Register<DivingModel>(Lifetime.Singleton);
        builder.Register<DivingGameplayBridge>(Lifetime.Singleton);
        builder.Register<GamePauseModel>(Lifetime.Singleton);

        builder.RegisterEntryPoint<DivingSimulationService>();
        builder.RegisterEntryPoint<PlayerRunSyncService>();

        RegisterUiIfPresent<UIManager>(builder);
        RegisterUiIfPresent<MainMenuView>(builder);
        RegisterUiIfPresent<HUDView>(builder);
        RegisterUiIfPresent<PauseView>(builder);
        RegisterUiIfPresent<ResultsView>(builder);

        if (HasUi<MainMenuView>())
            builder.RegisterEntryPoint<MainMenuPresenter>();

        if (HasUi<HUDView>())
            builder.RegisterEntryPoint<HUDPresenter>();

        if (HasUi<PauseView>())
            builder.RegisterEntryPoint<PausePresenter>();

        if (HasUi<ResultsView>())
            builder.RegisterEntryPoint<ResultsPresenter>();
    }

    private void RegisterUiIfPresent<T>(IContainerBuilder builder) where T : Component
    {
        var component = GetComponentInChildren<T>(true);
        if (component != null)
            builder.RegisterComponent(component);
    }

    private bool HasUi<T>() where T : Component
    {
        return GetComponentInChildren<T>(true) != null;
    }
}
