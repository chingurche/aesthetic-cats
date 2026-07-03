using UnityEngine;

[System.Serializable]
public class SpawnPrefab
{
    [Tooltip("Prefab that can be randomly selected and spawned.")]
    public GameObject Prefab;

    [Tooltip("Relative spawn weight. Higher values make this prefab appear more often than others in the same group.")]
    [Min(1)]
    public int Weight = 1;

    [Tooltip("Random scale multiplier range applied to this prefab. X = minimum multiplier, Y = maximum multiplier.")]
    public Vector2 ScaleMultiplierRange = new(0.9f, 1.1f);
}