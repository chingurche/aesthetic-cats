using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField]
    private List<SpawnableObject> spawnableObjects = new();

    private readonly Queue<GenerateRequest> generateQueue = new();
    private bool isGenerating;

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

    while (generateQueue.Count > 0)
    {
        GenerateRequest request = generateQueue.Dequeue();

        await Generate(
            request.MeshData,
            request.Mesh,
            request.Parent,
            request.WorldSeed,
            request.ChunkCoord);

        await UniTask.Yield();
    }

    isGenerating = false;
}

        private void GetRandomPointInCell(
        MeshData meshData,
        Mesh mesh,
        int centerX,
        int centerY,
        System.Random random,
        out Vector3 position,
        out Vector3 normal)
    {
        int width = Mathf.RoundToInt(Mathf.Sqrt(mesh.vertices.Length));

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
            a = mesh.vertices[v00];
            b = mesh.vertices[v10];
            c = mesh.vertices[v01];

            na = mesh.normals[v00];
            nb = mesh.normals[v10];
            nc = mesh.normals[v01];
        }
        else
        {
            a = mesh.vertices[v10];
            b = mesh.vertices[v11];
            c = mesh.vertices[v01];

            na = mesh.normals[v10];
            nb = mesh.normals[v11];
            nc = mesh.normals[v01];
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

    public class SpawnedObject
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
  
    List<SpawnedObject> spawnedObjects = new();

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
                mesh,
                parent,
                random,
                spawnedObjects);

            continue;
        }

    List<Vector3> clusterCenters = new();
    int clustersCreated = 0;
    int clusterAttempts = spawnable.ClusterCount * 10;

    while (clustersCreated < spawnable.ClusterCount &&
        clusterAttempts-- > 0)
    {
        int centerIndex = random.Next(meshData.vertices.Length);

        Vector3 center = meshData.vertices[centerIndex];

        if (center.y < spawnable.MinHeight ||
        center.y > spawnable.MaxHeight)
        {
            continue;
        }

        Vector3 centerNormal = mesh.normals[centerIndex];

        if (Vector3.Angle(centerNormal, Vector3.up) > spawnable.MaxSlope)
        {
            continue;
        }

        bool validCenter = true;

        foreach (Vector3 existingCenter in clusterCenters)
        {
            if ((center - existingCenter).sqrMagnitude < spawnable.MinDistanceBetweenClusters *
                spawnable.MinDistanceBetweenClusters)
            {
                validCenter = false;
                break;
            }
        }

        if (!validCenter)
            continue;

        clusterCenters.Add(center);

        
    float clusterRadius = Mathf.Lerp(
    spawnable.MinClusterRadius,
    spawnable.MaxClusterRadius,
    (float)random.NextDouble());

    float area = Mathf.PI * clusterRadius * clusterRadius;

    int amount = Mathf.Max(
        1,
        Mathf.RoundToInt(area * spawnable.ObjectsPerSquareMeter));

    await Spawn(
    spawnable,
    meshData,
    mesh,
    parent,
    random,
    spawnedObjects,
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
    Mesh mesh,
    Transform parent,
    System.Random random,
    List<SpawnedObject> spawnedObjects,
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

        int width = Mathf.RoundToInt(Mathf.Sqrt(mesh.vertices.Length));

        int centerX = -1;
        int centerY = -1;

        if (centerIndex != -1)
        {
            centerX = centerIndex % width;
            centerY = centerIndex / width;
        }

        int spawned = 0;
        int attempts = amount * 10;


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

                x = Mathf.Clamp(centerX + offsetX, 0, width - 2);
                y = Mathf.Clamp(centerY + offsetY, 0, width - 2);
            }
            else
            {
                x = random.Next(width - 1);
                y = random.Next(width - 1);
            }

            Vector3 position;
            Vector3 normal;

            GetRandomPointInCell(
                meshData,
                mesh,
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

        bool canSpawn = true;

        foreach (SpawnedObject other in spawnedObjects)
        {
            float minDistance =
                spawnable.SpawnRadius +
                other.Radius;

            if ((position - other.Position).sqrMagnitude <
                minDistance * minDistance)
            {
                canSpawn = false;
                break;
            }
        }
    
        if (!canSpawn)
            continue;

            // Проверка шанса появления
            if (random.NextDouble() > spawnable.SpawnChance)
            {
                continue;
            }

            GameObject prefab = GetRandomPrefab(spawnable, random);
    
            GameObject obj = Instantiate(prefab, parent);

            spawnedObjects.Add(
            new SpawnedObject(
                position,
                spawnable.SpawnRadius));


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
                    (float)random.NextDouble() * 360f,
                    Space.Self);
            }
            else
            {
                obj.transform.Rotate(
                    0f,
                    (float)random.NextDouble() * 360f,
                    0f);
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