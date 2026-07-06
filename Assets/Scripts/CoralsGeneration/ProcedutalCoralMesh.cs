using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteAlways]
public class ProceduralCoralMesh : MonoBehaviour
{
    [Header("Настройки формы")]
    public int maxDepth = 5;
    public int branchesPerNode = 3;
    public float initialRadius = 0.5f;
    public float initialLength = 2f;

    [Header("Настройки ветвления")]
    public float lengthMultiplier = 0.7f;
    public float scaleMultiplier = 0.6f;
    public float maxAngle = 45f;

    [Header("ОПТИМИЗАЦИЯ: Адаптивная детализация")]
    [Tooltip("Сколько граней у самого толстого основания (рекомендуется 6-8)")]
    public int maxSegments = 6;
    [Tooltip("Сколько граней у самых тонких кончиков (рекомендуется 3)")]
    public int minSegments = 3;

    [Header("Настройки цвета")]
    [Tooltip("Градиент цвета: слева - корень, справа - кончики")]
    public Gradient coralColorGradient = new Gradient();

    [Header("Случайность")]
    public int randomSeed = 0;

    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Color> colors;

    public void GenerateCoral()
    {
        ClearCoral();

        Random.InitState(randomSeed);

        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();

        BuildBranch(Vector3.zero, Quaternion.identity, initialRadius, initialLength, 0);

        Mesh coralMesh = new Mesh();
        coralMesh.name = "Procedural Coral Optimized";

        if (vertices.Count > 65000)
            coralMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        coralMesh.vertices = vertices.ToArray();
        coralMesh.triangles = triangles.ToArray();
        coralMesh.colors = colors.ToArray();
        coralMesh.RecalculateNormals();
        coralMesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = coralMesh;
    }

    public void ClearCoral()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh != null)
        {
            if (Application.isPlaying)
                Destroy(meshFilter.mesh);
            else
                DestroyImmediate(meshFilter.sharedMesh);

            meshFilter.sharedMesh = null;
        }
    }

    void BuildBranch(Vector3 startPos, Quaternion rotation, float radius, float length, int depth)
    {
        if (depth >= maxDepth) return;

        Vector3 endPos = startPos + rotation * Vector3.up * length;
        float nextRadius = radius * scaleMultiplier;

        int vIndex = vertices.Count;

        float depthRatio = (float)depth / Mathf.Max(1, maxDepth - 1);
        int currentSegments = Mathf.RoundToInt(Mathf.Lerp(maxSegments, minSegments, depthRatio));

        float angleStep = 360f / currentSegments;

        float currentDepthRatio = (float)depth / Mathf.Max(1, maxDepth);
        float nextDepthRatio = (float)(depth + 1) / Mathf.Max(1, maxDepth);

        Color baseColor = coralColorGradient.Evaluate(currentDepthRatio);
        Color tipColor = coralColorGradient.Evaluate(nextDepthRatio);

        for (int i = 0; i < currentSegments; i++)
        {
            Quaternion angleRot = Quaternion.AngleAxis(i * angleStep, Vector3.up);
            Vector3 localDir = angleRot * Vector3.forward;
            Vector3 worldDir = rotation * localDir;

            vertices.Add(startPos + worldDir * radius);
            colors.Add(baseColor);

            vertices.Add(endPos + worldDir * nextRadius);
            colors.Add(tipColor);
        }

        for (int i = 0; i < currentSegments; i++)
        {
            int nextI = (i + 1) % currentSegments;

            int baseA = vIndex + i * 2;
            int topA = baseA + 1;
            int baseB = vIndex + nextI * 2;
            int topB = baseB + 1;

            triangles.Add(baseA);
            triangles.Add(topA);
            triangles.Add(topB);

            triangles.Add(baseA);
            triangles.Add(topB);
            triangles.Add(baseB);
        }

        for (int i = 0; i < branchesPerNode; i++)
        {
            float angleX = Random.Range(-maxAngle, maxAngle);
            float angleZ = Random.Range(-maxAngle, maxAngle);
            float angleY = Random.Range(0f, 360f);

            Quaternion newRot = rotation * Quaternion.Euler(angleX, angleY, angleZ);
            BuildBranch(endPos, newRot, nextRadius, length * lengthMultiplier, depth + 1);
        }
    }
}