using System.Collections.Generic;
using UnityEngine;

public class AmbientRaysManager : MonoBehaviour
{
    [Header("Ray Spawner Settings")]
    [Tooltip("Сколько лучей одновременно находится вокруг игрока")]
    public int rayCount = 10;

    [Tooltip("Радиус вокруг камеры, где спавнятся лучи")]
    public float spawnRadius = 25f;

    [Tooltip("Высота (длина) каждого луча")]
    public float height = 30f;

    [Tooltip("Смещение по высоте. Уменьши (до минуса), чтобы лучи ушли глубже ко дну")]
    public float verticalOffset = -5f;

    [Header("Ray Shape & Sun Direction")]
    [Tooltip("Угол наклона солнца (X - вперед/назад, Y - вправо/влево)")]
    public Vector3 sunRotation = new Vector3(15f, -30f, 0f);

    [Tooltip("Множитель ширины луча сверху (делаем узким)")]
    public float topWidth = 0.15f;
    [Tooltip("Множитель ширины луча снизу (делаем широким)")]
    public float bottomWidth = 1.5f;

    [Header("Ray Appearance (Shader Settings)")]
    public float minScale = 1.5f;
    public float maxScale = 3.5f;

    [Tooltip("Цвет луча на самом верху. Альфа отвечает за общую яркость")]
    public Color baseColor = new Color(0.5f, 0.8f, 1f, 0.15f);

    [Tooltip("Цвет луча на глубине (абсорбция воды)")]
    public Color deepColor = new Color(0.05f, 0.2f, 0.5f, 0f);

    [Header("Caustics & Animation")]
    [Tooltip("Скорость, с которой лучи дрейфуют по течению")]
    public float driftSpeed = 0.3f;

    [Tooltip("Скорость переливания бликов внутри луча")]
    public float rippleSpeed = 1.2f;

    [Tooltip("Плотность бликов")]
    public float stripeDensity = 12f;

    [Tooltip("Резкость бликов. Выше = тоньше и острее струи света")]
    [Range(1f, 10f)] public float causticSharpness = 4.0f;

    [Header("Depth Fade")]
    [SerializeField] private float fadeStartDepth = 150f;
    [SerializeField] private float fadeEndDepth = 300f;

    [SerializeField] private Transform player;
    private class RayData
    {
        public GameObject gameObject;
        public Transform transform;
        public Material material;
        public float lifetime;
        public float maxLifetime;
        public Vector3 driftDir;
        public bool isMarkedForRemoval;
    }

    private List<RayData> activeRays = new List<RayData>();
    private GameObject container;
    private Mesh rayMesh;
    private Shader rayShader;

    private int currentTargetCount;

    void Start()
    {
        rayShader = Shader.Find("Custom/VolumetricRay");
        if (rayShader == null)
        {
            Debug.LogError("Шейдер 'Custom/VolumetricRay' не найден!");
            return;
        }

        container = new GameObject("WorldSpace_Rays");
        rayMesh = CreateRayMesh();
        currentTargetCount = rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            activeRays.Add(CreateNewRay(true));
        }
    }

    /// <summary>
    /// Метод для изменения количества лучей во время игры
    /// </summary>
    public void SetRayCount(int newCount)
    {
        rayCount = Mathf.Max(0, newCount);
        currentTargetCount = rayCount;

        int aliveCount = 0;
        foreach (var r in activeRays)
        {
            if (!r.isMarkedForRemoval) aliveCount++;
        }

        if (rayCount > aliveCount)
        {
            int toAdd = rayCount - aliveCount;
            for (int i = 0; i < toAdd; i++)
            {
                activeRays.Add(CreateNewRay(false));
            }
        }
        else if (rayCount < aliveCount)
        {
            int toRemove = aliveCount - rayCount;
            for (int i = activeRays.Count - 1; i >= 0; i--)
            {
                if (!activeRays[i].isMarkedForRemoval)
                {
                    activeRays[i].isMarkedForRemoval = true;
                    toRemove--;
                    if (toRemove <= 0) break;
                }
            }
        }
    }

    private RayData CreateNewRay(bool randomInitialLifetime)
    {
        GameObject rayObj = new GameObject("Ray_Instance");
        rayObj.transform.SetParent(container.transform);

        MeshFilter mf = rayObj.AddComponent<MeshFilter>();
        mf.sharedMesh = rayMesh;

        Material mat = new Material(rayShader);
        mat.SetColor("_Color", baseColor);
        mat.SetColor("_DeepColor", deepColor);
        mat.SetFloat("_Speed", rippleSpeed);
        mat.SetFloat("_Stripes", stripeDensity);
        mat.SetFloat("_Sharpness", causticSharpness);
        mat.SetFloat("_RandomSeed", Random.Range(0f, 1000f));

        MeshRenderer rend = rayObj.AddComponent<MeshRenderer>();
        rend.sharedMaterial = mat;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        RayData newRay = new RayData
        {
            gameObject = rayObj,
            transform = rayObj.transform,
            material = mat,
            isMarkedForRemoval = false
        };

        RespawnRay(newRay, randomInitialLifetime);
        return newRay;
    }

    private void RespawnRay(RayData ray, bool randomLifetime)
    {
        Vector2 randCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randCircle.x, verticalOffset, randCircle.y);
        ray.transform.position = spawnPos;

        float scale = Random.Range(minScale, maxScale);
        ray.transform.localScale = new Vector3(scale, height, 1f);

        ray.driftDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * driftSpeed;

        ray.maxLifetime = Random.Range(6f, 18f);
        ray.lifetime = randomLifetime ? Random.Range(0, ray.maxLifetime) : 0f;
    }

    void Update()
    {
        if (rayCount != currentTargetCount)
        {
            SetRayCount(rayCount);
        }

        float depth = WorldDepth.YToDepth(player.position.y);

        float depthFade = 1f - Mathf.Clamp01(Mathf.InverseLerp(fadeStartDepth, fadeEndDepth, depth));

        

        Vector3 camPos = transform.position;
        Vector3 rayUp = Quaternion.Euler(sunRotation) * Vector3.up;

        for (int i = activeRays.Count - 1; i >= 0; i--)
        {
            RayData ray = activeRays[i];

            if (depthFade <= 0.001f)
            {
                ray.gameObject.SetActive(false);
                continue;
            }
            else if (!ray.gameObject.activeSelf)
            {
                ray.gameObject.SetActive(true);
            }

            ray.lifetime += Time.deltaTime;
            float t = ray.lifetime / ray.maxLifetime;

            float dist = Vector2.Distance(new Vector2(camPos.x, camPos.z), new Vector2(ray.transform.position.x, ray.transform.position.z));

            if (t >= 1f || dist > spawnRadius + 5f)
            {
                if (ray.isMarkedForRemoval)
                {
                    Destroy(ray.material);
                    Destroy(ray.gameObject);
                    activeRays.RemoveAt(i);
                    continue;
                }
                else
                {
                    RespawnRay(ray, false);
                    t = 0f;
                }
            }

            float alphaMultiplier = 1f;
            if (t < 0.2f) alphaMultiplier = Mathf.Lerp(0f, 1f, t / 0.2f);
            else if (t > 0.8f) alphaMultiplier = Mathf.Lerp(1f, 0f, (t - 0.8f) / 0.2f);

            Color c = baseColor;
            c.a *= alphaMultiplier * depthFade;
            ray.material.SetColor("_Color", c);

            ray.transform.Translate(ray.driftDir * Time.deltaTime, Space.World);

            Vector3 forwardToCam = camPos - ray.transform.position;
            Vector3 right = Vector3.Cross(rayUp, forwardToCam);
            if (right.sqrMagnitude > 0.001f)
            {
                Vector3 faceDir = Vector3.Cross(right, rayUp);
                ray.transform.rotation = Quaternion.LookRotation(-faceDir, rayUp);
            }
        }
    }

    Mesh CreateRayMesh()
    {
        Mesh m = new Mesh();
        m.name = "ProceduralRayMesh";

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-0.5f * topWidth, 0.5f, 0);
        vertices[1] = new Vector3(0.5f * topWidth, 0.5f, 0);
        vertices[2] = new Vector3(-0.5f * bottomWidth, -0.5f, 0);
        vertices[3] = new Vector3(0.5f * bottomWidth, -0.5f, 0);

        int[] tris = new int[6] { 0, 1, 2, 2, 1, 3 };

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 1);
        uvs[1] = new Vector2(1, 1);
        uvs[2] = new Vector2(0, 0);
        uvs[3] = new Vector2(1, 0);

        Color[] colors = new Color[4];
        colors[0] = new Color(1, 1, 1, 1f);
        colors[1] = new Color(1, 1, 1, 1f);
        colors[2] = new Color(1, 1, 1, 0f);
        colors[3] = new Color(1, 1, 1, 0f);

        m.vertices = vertices;
        m.triangles = tris;
        m.uv = uvs;
        m.colors = colors;
        return m;
    }

    void OnDestroy()
    {
        if (activeRays != null)
        {
            foreach (var ray in activeRays)
            {
                if (ray.material != null) Destroy(ray.material);
                if (ray.gameObject != null) Destroy(ray.gameObject);
            }
            activeRays.Clear();
        }
        if (rayMesh != null) Destroy(rayMesh);
        if (container != null) Destroy(container);
    }
}