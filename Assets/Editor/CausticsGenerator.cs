using UnityEngine;
using UnityEditor;
using System.IO;

public class CausticsGenerator : MonoBehaviour
{
    struct PointData
    {
        public Vector2 center;
        public float radius;
        public float phase;
    }

    [MenuItem("Tools/Generate Caustics Sequence (30 Frames)")]
    public static void GenerateSequence()
    {
        int resolution = 512;
        int frames = 30;
        int numPoints = 35;

        float blurRadius = 0.12f;
        float glowContrast = 2.0f;

        PointData[] points = new PointData[numPoints];

        for (int i = 0; i < numPoints; i++)
        {
            points[i] = new PointData
            {
                center = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)),
                radius = Random.Range(0.02f, 0.08f),
                phase = Random.Range(0f, Mathf.PI * 2f)
            };
        }

        string folderPath = Application.dataPath + "/CausticsFrames";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        for (int f = 0; f < frames; f++)
        {
            float time = (float)f / frames;
            Vector2[] currentPoints = new Vector2[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                float angle = points[i].phase + time * Mathf.PI * 2f;
                float px = points[i].center.x + Mathf.Cos(angle) * points[i].radius;
                float py = points[i].center.y + Mathf.Sin(angle) * points[i].radius;

                currentPoints[i] = new Vector2(Mathf.Repeat(px, 1f), Mathf.Repeat(py, 1f));
            }

            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (float)x / resolution;
                    float ny = (float)y / resolution;

                    float minDist = 1f;
                    float secondMinDist = 1f;

                    for (int i = 0; i < numPoints; i++)
                    {
                        for (int offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            for (int offsetY = -1; offsetY <= 1; offsetY++)
                            {
                                Vector2 repeatPoint = currentPoints[i] + new Vector2(offsetX, offsetY);
                                float dist = Vector2.Distance(new Vector2(nx, ny), repeatPoint);

                                if (dist < minDist)
                                {
                                    secondMinDist = minDist;
                                    minDist = dist;
                                }
                                else if (dist < secondMinDist)
                                {
                                    secondMinDist = dist;
                                }
                            }
                        }
                    }

                    float edge = secondMinDist - minDist;
                    float t = Mathf.InverseLerp(blurRadius, 0f, edge);
                    float intensity = Mathf.Pow(t, glowContrast);

                    tex.SetPixel(x, y, new Color(intensity, intensity, intensity, 1f));
                }
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(folderPath + $"/CausticFrame_{f:D2}.png", bytes);

            EditorUtility.DisplayProgressBar("Генерация каустики", $"Кадр {f + 1} из {frames}", (float)f / frames);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        Debug.Log("Секвенция из 30 кадров успешно сгенерирована в папке CausticsFrames!");
    }
}