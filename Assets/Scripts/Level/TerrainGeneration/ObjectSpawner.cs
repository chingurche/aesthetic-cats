using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField]
    private List<SpawnableObject> spawnableObjects = new();
    
    private ObjectPool pool;
    private readonly Queue<GenerateRequest> generateQueue = new();
    private bool isGenerating;
    List<ClusterCenter> allClusterCenters = new();

    private readonly Dictionary<Vector2Int, List<SpawnedObject>> spatialGrid = new();

    private const float CellSize = 3f;

    private class ChunkObjects
    {
        public readonly List<GameObject> Objects = new();
    }

    private Vector2Int GetCell(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / CellSize),
            Mathf.FloorToInt(position.z / CellSize));
    }

    private void AddObject(SpawnedObject obj)
    {
        Vector2Int cell = GetCell(obj.Position);

        if (!spatialGrid.TryGetValue(cell, out var list))
        {
            list = new List<SpawnedObject>();
            spatialGrid[cell] = list;
        }

        list.Add(obj);
    }

    private bool CanSpawn(Vector3 position, float radius)
    {
        Vector2Int cell = GetCell(position);

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector2Int neighbour = new(cell.x + x, cell.y + y);

                if (!spatialGrid.TryGetValue(neighbour, out var objects))
                    continue;

                foreach (SpawnedObject other in objects)
                {
                    float minDistance = radius + other.Radius;

                    if ((position - other.Position).sqrMagnitude <
                        minDistance * minDistance)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void Awake()
    {
        pool = GetComponent<ObjectPool>();
    }
    private readonly Dictionary<Transform, ChunkObjects> chunkObjects = new();

    public void ReleaseChunkObjects(Transform parent)
    {
        if (!chunkObjects.TryGetValue(parent, out ChunkObjects chunk))
            return;

        foreach (GameObject obj in chunk.Objects)
        {
            pool.Release(obj);
        }

        chunk.Objects.Clear();
    }

    public class ClusterCenter
    {
        public Vector3 Position;
        public SpawnableObject Spawnable;
    }
    private class GenerateRequest
    {
        public MeshData MeshData;
        public Mesh Mesh;
        public Transform Parent;
        public int WorldSeed;
        public Vector2 ChunkCoord;
    }

    public void EnqueueGenerate(
    MeshData meshData,
    Mesh mesh,
    Transform parent,
    int worldSeed,
    Vector2 chunkCoord)
    {
        generateQueue.Enqueue(new GenerateRequest
        {
            MeshData = meshData,
            Mesh = mesh,
            Parent = parent,
            WorldSeed = worldSeed,
            ChunkCoord = chunkCoord
        });

        if (!isGenerating)
        {
            ProcessQueue().Forget();
        }
    }

   private async UniTaskVoid ProcessQueue()
    {
        isGenerating = true;

        float frameStart = Time.realtimeSinceStartup;
        const float frameBudget = 0.004f; // 4 мс

        while (generateQueue.Count > 0)
        {
            GenerateRequest request = generateQueue.Dequeue();

            await Generate(
                request.MeshData,
                request.Mesh,
                request.Parent,
                request.WorldSeed,
                request.ChunkCoord);

            if (Time.realtimeSinceStartup - frameStart > frameBudget)
            {
                frameStart = Time.realtimeSinceStartup;
                await UniTask.Yield();
            }
        }

        isGenerating = false;
    }

        private void GetRandomPointInCell(
            Vector3[] vertices,
            Vector3[] normals,
            int width,
            int centerX,
            int centerY,
            System.Random random,
            out Vector3 position,
            out Vector3 normal)
    {

        centerX = Mathf.Clamp(centerX, 0, width - 2);
        centerY = Mathf.Clamp(centerY, 0, width - 2);

        int v00 = centerY * width + centerX;
        int v10 = centerY * width + centerX + 1;
        int v01 = (centerY + 1) * width + centerX;
        int v11 = (centerY + 1) * width + centerX + 1;

        Vector3 a, b, c;
        Vector3 na, nb, nc;

        if (random.NextDouble() < 0.5)
        {
            a = vertices[v00];
            b = vertices[v10];
            c = vertices[v01];

            na = normals[v00];
            nb = normals[v10];
            nc = normals[v01];
        }
        else
        {
            a = vertices[v10];
            b = vertices[v11];
            c = vertices[v01];

            na = normals[v10];
            nb = normals[v11];
            nc = normals[v01];
        }

        float u = (float)random.NextDouble();
        float v = (float)random.NextDouble();

        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }

        position = a + (b - a) * u + (c - a) * v;
        normal = (na + (nb - na) * u + (nc - na) * v).normalized;
    }

    private GameObject GetRandomPrefab(
    SpawnableObject spawnable,
    System.Random random)
    {
        int totalWeight = 0;

        foreach (var prefab in spawnable.Prefabs)
        {
            totalWeight += prefab.Weight;
        }

        int value = random.Next(totalWeight);

        foreach (var prefab in spawnable.Prefabs)
        {
            value -= prefab.Weight;

            if (value < 0)
            {
                return prefab.Prefab;
            }
        }

        return spawnable.Prefabs[0].Prefab;
    }

    public struct SpawnedObject
    {
        public Vector3 Position;
        public float Radius;

        public SpawnedObject(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }

   
    public async UniTask Generate(
    MeshData meshData,
    Mesh mesh,
    Transform parent,
    int worldSeed,
    Vector2 chunkCoord)
    {

    Vector3[] vertices = mesh.vertices;
    Vector3[] normals = mesh.normals;

    int width = Mathf.RoundToInt(Mathf.Sqrt(vertices.Length));

    if (!chunkObjects.TryGetValue(parent, out ChunkObjects chunk))
    {
        chunk = new ChunkObjects();
        chunkObjects[parent] = chunk;
    }

    foreach (GameObject obj in chunk.Objects)
    {
        pool.Release(obj);
    }

    chunk.Objects.Clear();

    allClusterCenters.Clear();

    spatialGrid.Clear();

    int chunkSeed =
        worldSeed ^
        ((int)chunkCoord.x * 74851083) ^
        ((int)chunkCoord.y * 19345863);

    System.Random random = new System.Random(chunkSeed);
   
    foreach (var spawnable in spawnableObjects)
    {
       
       if (!spawnable.UseClusters)
        {
            await Spawn(
                spawnable,
                meshData,
                vertices,
                normals,
                width,
                parent,
                chunk,
                random
                );

            continue;
        }

    int clustersCreated = 0;
    int clusterAttempts = spawnable.ClusterCount * 10;

    while (clustersCreated < spawnable.ClusterCount &&
        clusterAttempts-- > 0)
    {
        int border = Mathf.CeilToInt(spawnable.MaxClusterRadius);

        int centerX = random.Next(border, width - border);
        int centerY = random.Next(border, width - border);

        int centerIndex = centerY * width + centerX;

        Vector3 center = meshData.vertices[centerIndex];

        if (center.y < spawnable.MinHeight ||
        center.y > spawnable.MaxHeight)
        {
            continue;
        }

        Vector3 centerNormal = normals[centerIndex];

        if (Vector3.Angle(centerNormal, Vector3.up) > spawnable.MaxSlope)
        {
            continue;
        }

        bool validCenter = true;

        foreach (ClusterCenter other in allClusterCenters)
        {
            float minDistance =
            other.Spawnable == spawnable
                ? spawnable.MinDistanceBetweenClusters
                : Mathf.Max(
                    spawnable.MinDistanceToOtherClusters,
                    other.Spawnable.MinDistanceToOtherClusters);

            if ((center - other.Position).sqrMagnitude < minDistance * minDistance)
            {
                validCenter = false;
                break;
            }
        }

        if (!validCenter)
            continue;

        allClusterCenters.Add(new ClusterCenter
        {
            Position = center,
            Spawnable = spawnable
        });

    float clusterRadius = Mathf.Lerp(
    spawnable.MinClusterRadius,
    spawnable.MaxClusterRadius,
    (float)random.NextDouble());

    const float packing = 0.55f;

    float area = Mathf.PI * clusterRadius * clusterRadius;

    float objectArea = Mathf.PI * spawnable.ObjectSpacing * spawnable.ObjectSpacing;

    int amount = Mathf.Max(1, Mathf.CeilToInt(area / objectArea * packing));

    await Spawn(
    spawnable,
    meshData,
    vertices,
    normals,
    width,
    parent,
    chunk,
    random,
    centerIndex,
    amount,
    clusterRadius);

    clustersCreated++;
        }
    }
   

}
  private async UniTask Spawn(
    SpawnableObject spawnable,
    MeshData meshData,
    Vector3[] vertices,
    Vector3[] normals,
    int width,
    Transform parent,
    ChunkObjects chunk,
    System.Random random,
    int centerIndex = -1,
    int amount = -1,
    float clusterRadius = 0)
    {

        float frameStart = Time.realtimeSinceStartup;
        const float frameBudget = 0.003f; // 3 миллисекунды  

        if (amount == -1)
        {
            amount = spawnable.MaxPerChunk;
        }

        int centerX = -1;
        int centerY = -1;

        if (centerIndex != -1)
        {
            centerX = centerIndex % width;
            centerY = centerIndex / width;
        }

        int spawned = 0;
        int attempts = Mathf.Min(Mathf.RoundToInt(clusterRadius * clusterRadius * 2f), 700);



        while (spawned < amount && attempts-- > 0)
        {

            int x;
            int y;

            if (centerIndex != -1)
            {

                float angle = (float)random.NextDouble() * Mathf.PI * 2f;

                float t = (float)random.NextDouble();

                float exponent = Mathf.Lerp(
                    1f,
                    3f,
                    spawnable.ClusterFalloff);

                float radius = Mathf.Pow(t, exponent) * clusterRadius;

                float noise = Mathf.Lerp(
                    1f - spawnable.ClusterNoise,
                    1f + spawnable.ClusterNoise,
                    (float)random.NextDouble());

                radius *= noise;

                int offsetX = Mathf.RoundToInt(Mathf.Cos(angle) * radius);
                int offsetY = Mathf.RoundToInt(Mathf.Sin(angle) * radius);

                x = centerX + offsetX;
                y = centerY + offsetY;

                if (x < 0 || x >= width - 1 ||
                    y < 0 || y >= width - 1)
                {
                    continue;
                }
            }
            else
            {
                x = random.Next(width - 1);
                y = random.Next(width - 1);
            }

            Vector3 position;
            Vector3 normal;

           GetRandomPointInCell(
                vertices,
                normals,
                width,
                x,
                y,
                random,
                out position,
                out normal);
            // Проверка высоты
            if (position.y < spawnable.MinHeight ||
                position.y > spawnable.MaxHeight)
            {
                continue;
            }

            // Проверка уклона
            float slope = Vector3.Angle(normal, Vector3.up);

            if (slope > spawnable.MaxSlope)
            {
                continue;
            }

       

            // Проверка шанса появления
            if (random.NextDouble() > spawnable.SpawnChance)
            {
                continue;
            }

            if (!CanSpawn(position, spawnable.SpawnRadius))
            continue;

            GameObject prefab = GetRandomPrefab(spawnable, random);
    
            GameObject obj = pool.Get(prefab, parent);
            Renderer renderer = obj.GetComponent<Renderer>();
            
            if (renderer != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);

                Color tint = Color.HSVToRGB(
                    (float)random.NextDouble(),
                    0.8f,
                    1f);

                block.SetColor("_Color", tint);

                renderer.SetPropertyBlock(block);
            }
            chunk.Objects.Add(obj);

            SpawnedObject spawnedObject = new SpawnedObject(
            position,
            spawnable.SpawnRadius);

            AddObject(spawnedObject);


            obj.transform.localPosition = position;

            // Масштаб
            float multiplier = Mathf.Lerp(
                spawnable.MinScaleMultiplier,
                spawnable.MaxScaleMultiplier,
                (float)random.NextDouble());

            obj.transform.localScale = spawnable.Scale * multiplier;

            // Выравнивание по поверхности
            if (spawnable.AlignToSurface)
            {
                obj.transform.up = normal;

                obj.transform.Rotate(
                    Vector3.up,
                    (float)random.NextDouble() * 360f, Space.Self);
            }
            else
            {
                obj.transform.Rotate(0f, (float)random.NextDouble() * 360f, 0f);
            }

            spawned++;

            if (Time.realtimeSinceStartup - frameStart > frameBudget)
            {
                frameStart = Time.realtimeSinceStartup;
                await UniTask.Yield();
            }
        }
    }
}