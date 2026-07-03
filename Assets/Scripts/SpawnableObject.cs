using UnityEngine;

[CreateAssetMenu(menuName = "Terrain Generation/Spawnable Object")]
public class SpawnableObject : ScriptableObject
{
    [Tooltip("List of prefabs that can be randomly selected during spawning.")]
    public SpawnPrefab[] Prefabs;

    [Header("Spawn Settings")]

    [Tooltip("Probability that a valid spawn point will create this object. 0 = never, 1 = always.")]
    [Range(0, 1)]
    public float SpawnChance = 0.5f;

    [Tooltip("Maximum number of objects spawned per chunk when clusters are disabled.")]
    public int MaxPerChunk = 10;

    [Header("Terrain Restrictions")]

    [Tooltip("Minimum terrain height where this object can spawn.")]
    public float MinHeight = -100;

    [Tooltip("Maximum terrain height where this object can spawn.")]
    public float MaxHeight = 100;

    [Tooltip("Maximum allowed terrain slope in degrees. Objects will not spawn on steeper surfaces.")]
    [Range(0, 90)]
    public float MaxSlope = 30f;

    [Header("Collision")]

    [Tooltip("Minimum spacing radius around this object. Prevents objects from overlapping.")]
    public float SpawnRadius = 1f;

    [Header("Scale")]

    [Tooltip("Base scale applied to every spawned object.")]
    public Vector3 Scale = Vector3.one;

    [Tooltip("Minimum random scale multiplier applied to Scale.")]
    [Min(0)]
    public float MinScaleMultiplier = 1f;

    [Tooltip("Maximum random scale multiplier applied to Scale.")]
    [Min(0)]
    public float MaxScaleMultiplier = 1f;

    [Header("Clusters")]

    [Tooltip("If enabled, objects are spawned in clusters instead of being distributed across the whole chunk.")]
    public bool UseClusters = false;

    [Tooltip("Number of clusters generated per chunk.")]
    [Min(1)]
    public int ClusterCount = 3;

    [Tooltip("Average number of objects per square meter inside a cluster.")]
    [Min(0.1f)]
    public float ObjectsPerSquareMeter = 0.3f;

    [Tooltip("Minimum radius of a generated cluster.")]
    [Min(0)]
    public float MinClusterRadius = 4f;

    [Tooltip("Maximum radius of a generated cluster.")]
    [Min(0)]
    public float MaxClusterRadius = 8f;

    [Tooltip("Minimum distance allowed between cluster centers.")]
    [Min(0)]
    public float MinDistanceBetweenClusters = 15f;

    [Header("Cluster Shape")]

    [Tooltip("Controls how strongly objects are concentrated towards the cluster center. 0 = uniform, 1 = dense center.")]
    [Range(0f, 1f)]
    public float ClusterFalloff = 0.5f;

    [Tooltip("Adds randomness to the cluster shape. Higher values make cluster borders less regular.")]
    [Range(0f, 1f)]
    public float ClusterNoise = 0.3f;

    [Header("Orientation")]

    [Tooltip("Rotate the object so that its up direction matches the terrain normal.")]
    public bool AlignToSurface = true;
}