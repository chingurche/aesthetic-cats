using VContainer;
using VContainer.Unity;
using UnityEngine;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log("🔧 GameLifetimeScope: регистрация сервисов...");
        builder.Register<AudioSystem>(Lifetime.Singleton); // AudioSource ГЕНЕРИТЬ ПРЯМ ТАМ!!!
        builder.Register<SaveSystem>(Lifetime.Singleton);
        Debug.Log("✅ GameLifetimeScope: сервисы зарегистрированы!");
    }
}
