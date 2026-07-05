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
    
    private readonly Queue<Vector2> chunkCreationQueue = new();

    private bool isCreatingChunks;
    
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

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

    private async UniTask CreateQueuedChunks()
    {
        isCreatingChunks = true;

        Stopwatch stopwatch = Stopwatch.StartNew();

        long frameBudgetTicks = Stopwatch.Frequency / 1000 * 3;

        while (chunkCreationQueue.Count > 0)
        {

            
            Vector2 coord = chunkCreationQueue.Dequeue();

            if (terrainChunkDictionary[coord] != null)
            {
                continue;
            }

            terrainChunkDictionary[coord] =
                new TerrainChunk(
                    coord,
                    chunkSize,
                    transform,
                    mapMaterial,
                    objectSpawner,
                    mapGenerator);

            if (stopwatch.ElapsedTicks >= frameBudgetTicks)
            {
                stopwatch.Restart();
                await UniTask.Yield();
            }
        }

        isCreatingChunks = false;
    }

    public void FixedUpdate()
    {
        if (!isInitialized) return;

        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();

        if (!isCreatingChunks)
        {
            CreateQueuedChunks().Forget();
        }
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.TryGetValue(viewedChunkCoord, out TerrainChunk chunk))
                {
                    if (chunk != null)
                    {
                        chunk.UpdateTerrainChunk();

                        if (chunk.IsVisible())
                        {
                            terrainChunksVisibleLastUpdate.Add(chunk);
                        }
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, null);
                    chunkCreationQueue.Enqueue(viewedChunkCoord);
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MapData mapData;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        private readonly MapGenerator mapGenerator;
        private readonly ObjectSpawner objectSpawner;
        private readonly Vector2 coord;
        private bool visible;

        public TerrainChunk(
            Vector2 coord, 
            int size, 
            Transform parent, 
            Material material, 
            ObjectSpawner objectSpawner,
            MapGenerator mapGenerator)
        {
            this.mapGenerator = mapGenerator;
            this.objectSpawner = objectSpawner;
            this.coord = coord;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
           
            meshObject = new GameObject($"Chunk {coord.x} {coord.y}");
            meshObject.layer = 3; // 3: Ground
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);
            
            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

     

        void OnMeshDataReceived(MeshData meshData)
        {
            Mesh mesh = meshData.CreateMesh();
            
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;

            objectSpawner.Generate(
            meshData,
            mesh,
            meshObject.transform,
            mapGenerator.Seed,
            coord).Forget();
                
        }
        public void UpdateTerrainChunk()
        {
            float vieweDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = vieweDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshRenderer.enabled = visible;
            meshCollider.enabled = visible;
            this.visible = visible;
        }

        public bool IsVisible()
        {
            return visible;
        }
    }
}
