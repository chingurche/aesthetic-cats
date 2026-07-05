using VContainer;
using VContainer.Unity;
using UnityEngine;

public class LevelLifetimeScope : LifetimeScope
{

    protected override void Configure(IContainerBuilder builder)
    {   
        builder.RegisterComponentInHierarchy<LevelManager>();
    }
}
