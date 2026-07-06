using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Diagnostics;
using VContainer;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDst = 300;
    private Transform viewer;

    public Material mapMaterial;
    public static Vector2 viewerPosition;
    private int chunkSize;
    int chunkVisibleInViewDst;
    private readonly Dictionary<Vector2Int, TerrainChunk> activeChunks = new();
    private readonly Queue<TerrainChunk> chunkPool = new();
    private readonly List<Vector2Int> chunksToRelease = new();
    private readonly HashSet<Vector2Int> requiredCoords = new();
    private MapGenerator mapGenerator;
    private ObjectSpawner objectSpawner;

    private bool isInitialized = false;
    

    public void Initialize()
    {
        mapGenerator = GetComponent<MapGenerator>();
        objectSpawner = GetComponent<ObjectSpawner>();

        viewer = GameObject.FindGameObjectWithTag("Player").transform;

        chunkSize = MapGenerator.mapChunkSize - 1;
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        isInitialized = true;

        
    }

    public void SetViewer(Transform playerTransform)
    {
        viewer = playerTransform;
    }

    public void Update()
    {
        if (!isInitialized) return;

        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

   private void UpdateVisibleChunks()
    {

        requiredCoords.Clear();
        chunksToRelease.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
            {
                Vector2Int coord = new(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                requiredCoords.Add(coord);

                if (!activeChunks.TryGetValue(coord, out TerrainChunk chunk))
                {
                    chunk = GetChunk();

                    chunk.Initialize(
                        coord,
                        chunkSize,
                        transform,
                        mapMaterial,
                        objectSpawner,
                        mapGenerator);

                    activeChunks.Add(coord, chunk);

                    chunk.UpdateTerrainChunk();
                }
                else
                {
                    chunk.UpdateTerrainChunk();
                }
            }
        }

        chunksToRelease.Clear();

        foreach (var pair in activeChunks)
        {
            if (!requiredCoords.Contains(pair.Key))
            {
                chunksToRelease.Add(pair.Key);
            }
        }

        foreach (Vector2Int coord in chunksToRelease)
        {
            ReleaseChunk(activeChunks[coord]);
        }
    }

    private void ReleaseChunk(TerrainChunk chunk)
    {
        chunk.Release();

        activeChunks.Remove(chunk.Coord);

        chunkPool.Enqueue(chunk);
    }
    private TerrainChunk GetChunk()
    {
        if (chunkPool.Count > 0)
            return chunkPool.Dequeue();

        return TerrainChunk.Create(
            transform,
            mapMaterial);
    }
   
    public class TerrainChunk
    {
    public Vector2Int Coord { get; private set; }
    public Bounds Bounds => bounds;

    private GameObject meshObject;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private Bounds bounds;
    private Vector2 position;

    private bool visible;

    private MapGenerator mapGenerator;
    private ObjectSpawner objectSpawner;

    private TerrainChunk() { }

    public static TerrainChunk Create(Transform parent, Material material)
    {
        TerrainChunk chunk = new TerrainChunk();

        chunk.meshObject = new GameObject("Terrain Chunk");
        chunk.meshObject.layer = 3;

        chunk.meshRenderer =
            chunk.meshObject.AddComponent<MeshRenderer>();

        chunk.meshFilter =
            chunk.meshObject.AddComponent<MeshFilter>();

        chunk.meshCollider =
            chunk.meshObject.AddComponent<MeshCollider>();

        chunk.meshRenderer.material = material;

        chunk.meshObject.transform.SetParent(parent, false);
        chunk.meshObject.SetActive(false);

        return chunk;
    }
        public void Initialize(
            Vector2Int coord,
            int size,
            Transform parent,
            Material material,
            ObjectSpawner spawner,
            MapGenerator generator)
        {
            Coord = coord;

            mapGenerator = generator;
            objectSpawner = spawner;

            position = coord * size;

            Vector2 center = position + Vector2.one * size * 0.5f;
            bounds = new Bounds(center, Vector3.one * size);

            meshObject.transform.SetParent(parent, false);
            meshObject.transform.position = new Vector3(position.x, 0, position.y);

            meshRenderer.material = material;

            mapGenerator.RequestMapData(position, OnMapDataReceived);

            

        }

        private void OnMapDataReceived(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        private void OnMeshDataReceived(MeshData meshData)
        {
            Mesh mesh = meshData.CreateMesh();


            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;

            meshObject.SetActive(true);

            objectSpawner.EnqueueGenerate(
                meshData,
                mesh,
                meshObject.transform,
                mapGenerator.Seed,
                Coord);
        }

        public void UpdateTerrainChunk()
        {
            float dst = Mathf.Sqrt(bounds.SqrDistance(EndlessTerrain.viewerPosition));

            SetVisible(dst <= EndlessTerrain.maxViewDst);
        }

        public void SetVisible(bool value)
        {
            visible = value;
            meshObject.SetActive(value);
        }

        public bool IsVisible()
        {
            return visible;
        }

        public void Release()
        {
            meshObject.SetActive(false);
            objectSpawner.ReleaseChunkObjects(meshObject.transform);

            meshCollider.sharedMesh = null;
            meshFilter.sharedMesh = null;
            visible = false;

            meshObject.SetActive(false);
        }
    } 
}