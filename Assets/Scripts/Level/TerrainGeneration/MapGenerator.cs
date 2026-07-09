using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
	public Noise.NormalizeMode normalizeMode;

	public const int mapChunkSize = 128;

    [Header("Noise")]
	public float noiseScale;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;
    public float meshHeightMultiplier;
	
    [Header("Slope Settings")]
    public Vector2 slopeDirection = new Vector2(1, 0); // Направление склона
    public float slopeIntensity = 2f;

	public int Seed => seed;

    [Header("Update")]
	public bool autoUpdate;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	public void DrawMapInEditor()
	{
		MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = GetComponent<MapDisplay>();

        display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier));
	}

	public void RequestMapData(Vector2 center, Action<MapData> callback)
	{
		ThreadStart threadStart = delegate
		{
			MapDataThread(center, callback);
		};

		new Thread(threadStart).Start();
	}

	private void MapDataThread(Vector2 center, Action<MapData> callback)
	{
		MapData mapData = GenerateMapData(center);
		lock (mapDataThreadInfoQueue)
		{
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
		}
	}

	public void RequestMeshData(MapData mapData, Action<MeshData> callback)
	{
		ThreadStart threadStart = delegate {
			MeshDataThread (mapData, callback);
		};

		new Thread (threadStart).Start ();
	}

	public void MeshDataThread(MapData mapData, Action<MeshData> callback)
	{
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier);
		lock (meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData)); 
		}
	}

	void Update()
	{
		if (mapDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
		if (meshDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
	}

	private MapData GenerateMapData(Vector2 center) {
		float[,] noiseMap = Noise.GenerateNoiseMap (
			mapChunkSize, 
			mapChunkSize, 
			seed, 
			noiseScale, 
			octaves, 
			persistance, 
			lacunarity, 
			center + offset, 
			normalizeMode, 
			slopeDirection, 
			slopeIntensity);

		return new MapData(noiseMap);
    }

	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}
}

public struct MapData
{
	public float[,] heightMap;

	public MapData(float[,] heightMap)
	{
		this.heightMap = heightMap;
	}
}