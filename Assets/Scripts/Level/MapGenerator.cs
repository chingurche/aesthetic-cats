using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour
{
	private const int mapChunkSize = 128;
	public float noiseScale;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;
    public float meshHeightMultiplier;

	public bool autoUpdate;

	public void GenerateMap() {
		float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
        //display.DrawnNoiseMap(noiseMap);

        MapDisplay display = GetComponent<MapDisplay>();
        display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier));

    }
}
