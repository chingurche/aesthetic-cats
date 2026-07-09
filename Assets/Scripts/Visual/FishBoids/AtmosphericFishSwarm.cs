using UnityEngine;

public class AtmosphericFishSwarm : MonoBehaviour
{
    [Header("Настройки стаи")]
    public Mesh fishMesh;
    public Material fishMaterial;
    [Range(5, 2000)]
    public int fishCount = 30;

    [Header("Размер рыб")]
    [Tooltip("Минимальный масштаб рыбы")]
    public float minScale = 0.8f;
    [Tooltip("Максимальный масштаб рыбы")]
    public float maxScale = 1.2f;

    [Header("Настройки Боидов")]
    public float maxSpeed = 5f;
    public float perceptionRadius = 3f;
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;

    [Header("Зона плавания")]
    public float boundsRadius = 20f;
    public float boundsWeight = 2f;

    [Header("Избегание дна (Ground)")]
    [Tooltip("Укажите слой, на котором находится ваш ландшафт/дно")]
    public LayerMask groundLayer;
    [Tooltip("За сколько метров рыба замечает дно")]
    public float avoidGroundDistance = 3f;
    [Tooltip("Насколько сильно рыбу выталкивает вверх")]
    public float avoidGroundWeight = 15f;

    [Header("Взаимодействие с игроком")]
    [Tooltip("Перетащите сюда объект игрока (Transform)")]
    public Transform playerTransform;
    [Tooltip("Радиус испуга (от центра стаи до игрока)")]
    public float scareDistance = 15f;
    [Tooltip("Во сколько раз быстрее плывут испуганные рыбы")]
    public float scareSpeedMultiplier = 3f;

    [Header("Жизненный цикл")]
    [Tooltip("Если включено - рыбы вернутся. Если выключено - стая удалится после побега")]
    public bool returnIfNotScared = true;
    [Tooltip("Сколько секунд рыбы плывут в панике перед исчезновением/возвратом")]
    public float timeToFlee = 5f;

    [Header("Оптимизация")]
    [Tooltip("Дистанция, на которой стая перестает обновляться и рендериться")]
    public float maxUpdateDistance = 80f;
    [Tooltip("Если рыб больше этого числа, включается многопоточность (Parallel.For)")]
    public int parallelThreshold = 150;

    [Header("Коррекция модели")]
    [Tooltip("Если рыбы плывут боком, поменяйте Y на 90 или -90")]
    public Vector3 modelRotationOffset = new Vector3(0, -90f, 0);
    [Tooltip("Скорость плавного поворота модели")]
    public float rotationSpeed = 5f;

    private Matrix4x4[] matrices;
    private Vector3[] positions;
    private Vector3[] velocities;
    private Vector3[] newVelocities;
    private Quaternion[] rotations;
    private float[] scales;
    private float[] groundPushForces;

    private bool isCurrentlyScared = false;
    private float currentFleeTime = 0f;
    private float sqrMaxUpdateDistance;
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        sqrMaxUpdateDistance = maxUpdateDistance * maxUpdateDistance;

        if (matrices == null || matrices.Length != fishCount)
        {
            Initialize(fishCount);
        }
    }

    public void ResetSwarm()
    {
        isCurrentlyScared = false;
        currentFleeTime = 0f;
    }

    public void Initialize(int count)
    {
        isCurrentlyScared = false;
        currentFleeTime = 0f;
        fishCount = count;

        matrices = new Matrix4x4[fishCount];
        positions = new Vector3[fishCount];
        velocities = new Vector3[fishCount];
        newVelocities = new Vector3[fishCount];
        rotations = new Quaternion[fishCount];
        scales = new float[fishCount];
        groundPushForces = new float[fishCount];

        for (int i = 0; i < fishCount; i++)
        {
            positions[i] = transform.position + Random.insideUnitSphere * boundsRadius;
            velocities[i] = Random.onUnitSphere * maxSpeed;
            scales[i] = Random.Range(minScale, maxScale);
            groundPushForces[i] = 0f;

            Quaternion lookRot = Quaternion.LookRotation(velocities[i]);
            rotations[i] = lookRot * Quaternion.Euler(modelRotationOffset);
        }
    }

    void Update()
    {
        Vector3 centerPos = transform.position;
        Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;

        Transform targetForCulling = playerTransform != null ? playerTransform : mainCam.transform;
        if (targetForCulling != null)
        {
            if ((centerPos - targetForCulling.position).sqrMagnitude > sqrMaxUpdateDistance)
                return;
        }

        float dt = Time.deltaTime;
        bool playerIsClose = false;

        if (playerTransform != null && (centerPos - playerPos).sqrMagnitude < (scareDistance * scareDistance))
        {
            playerIsClose = true;
        }

        if (playerIsClose && !isCurrentlyScared)
        {
            isCurrentlyScared = true;
            currentFleeTime = 0f;
        }

        if (isCurrentlyScared)
        {
            currentFleeTime += dt;
            if (currentFleeTime >= timeToFlee)
            {
                if (!returnIfNotScared)
                {
                    gameObject.SetActive(false);
                    return;
                }
                else if (!playerIsClose)
                {
                    isCurrentlyScared = false;
                }
            }
        }

        if (fishCount >= parallelThreshold)
        {
            System.Threading.Tasks.Parallel.For(0, fishCount, i =>
            {
                CalculateFishVelocity(i, dt, playerPos, centerPos);
            });
        }
        else
        {
            for (int i = 0; i < fishCount; i++)
            {
                CalculateFishVelocity(i, dt, playerPos, centerPos);
            }
        }

        for (int i = 0; i < fishCount; i++)
        {
            velocities[i] = newVelocities[i];

            if ((Time.frameCount + i) % 4 == 0)
            {
                if (Physics.Raycast(positions[i], Vector3.down, out RaycastHit hit, avoidGroundDistance, groundLayer))
                {
                    groundPushForces[i] = 1f - (hit.distance / avoidGroundDistance);
                }
                else
                {
                    groundPushForces[i] = 0f;
                }
            }

            if (groundPushForces[i] > 0)
            {
                velocities[i] += Vector3.up * groundPushForces[i] * avoidGroundWeight * dt;
            }

            positions[i] += velocities[i] * dt;

            if (velocities[i] != Vector3.zero)
            {
                Quaternion targetLookRot = Quaternion.LookRotation(velocities[i]);
                Quaternion targetRot = targetLookRot * Quaternion.Euler(modelRotationOffset);
                rotations[i] = Quaternion.Slerp(rotations[i], targetRot, dt * rotationSpeed);
            }

            matrices[i] = Matrix4x4.TRS(positions[i], rotations[i], Vector3.one * scales[i]);
        }

        Graphics.DrawMeshInstanced(fishMesh, 0, fishMaterial, matrices, fishCount);
    }

    private void CalculateFishVelocity(int i, float dt, Vector3 playerPos, Vector3 centerPos)
    {
        if (isCurrentlyScared)
        {
            Vector3 fleeDir = positions[i] - playerPos;
            if (fleeDir == Vector3.zero) fleeDir = Vector3.up;

            float chaosX = (float)System.Math.Sin(i * 12.34f);
            float chaosY = (float)System.Math.Cos(i * 56.78f);
            float chaosZ = (float)System.Math.Sin(i * 90.12f);
            Vector3 chaos = new Vector3(chaosX, chaosY, chaosZ) * 0.6f;

            Vector3 targetVelocity = (fleeDir.normalized + chaos).normalized * (maxSpeed * scareSpeedMultiplier);
            newVelocities[i] = Vector3.Lerp(velocities[i], targetVelocity, dt * 2.5f);
            return;
        }

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int numNeighbors = 0;

        for (int j = 0; j < fishCount; j++)
        {
            if (i == j) continue;

            Vector3 offset = positions[j] - positions[i];
            float sqrDst = offset.sqrMagnitude;

            if (sqrDst < perceptionRadius * perceptionRadius)
            {
                float dst = (float)System.Math.Sqrt(sqrDst);
                separation -= offset / dst;
                alignment += velocities[j];
                cohesion += positions[j];
                numNeighbors++;
            }
        }

        if (numNeighbors > 0)
        {
            alignment /= numNeighbors;
            cohesion = (cohesion / numNeighbors) - positions[i];

            separation = separation.normalized * separationWeight;
            alignment = alignment.normalized * alignmentWeight;
            cohesion = cohesion.normalized * cohesionWeight;
        }

        Vector3 boundsForce = Vector3.zero;
        Vector3 centerOffset = centerPos - positions[i];
        if (centerOffset.sqrMagnitude > boundsRadius * boundsRadius)
        {
            boundsForce = centerOffset.normalized * boundsWeight;
        }

        newVelocities[i] = velocities[i] + (separation + alignment + cohesion + boundsForce) * dt;

        if (newVelocities[i].sqrMagnitude > maxSpeed * maxSpeed)
        {
            newVelocities[i] = newVelocities[i].normalized * maxSpeed;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, boundsRadius);
    }
}