using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelLifetimeScope : LifetimeScope
{
    protected override LifetimeScope FindParent()
    {
        return FindAnyObjectByType<GameLifetimeScope>();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<LevelManager>();
    }
}
