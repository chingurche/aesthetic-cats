using UnityEngine;

public class LootLightning : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 5f; // Скорость разгорания/затухания при взгляде

    private Renderer meshRenderer;
    private Material lootMaterial;

    // Кэшируем ID свойства шейдера для оптимизации
    private static readonly int FocusIntensityID = Shader.PropertyToID("_FocusIntensity");

    private float currentFocus = 0f;
    private bool isPlayerLooking = false;

    void Start()
    {
        // 1. Сначала пытаемся найти Renderer на самом корневом объекте
        meshRenderer = GetComponent<Renderer>();

        // 2. Если на корневом объекте рендера нет, ищем в дочерних объектах (children)
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<Renderer>();
        }

        // 3. Проверяем, нашли ли хоть что-то в итоге
        if (meshRenderer != null)
        {
            // Создаем уникальную копию материала для этого объекта
            lootMaterial = meshRenderer.material;
        }
        else
        {
            // Выводим ошибку с именем объекта, чтобы легче было искать в сцене
            Debug.LogError($"[{gameObject.name}] Renderer не найден ни на самом объекте, ни в его дочерних объектах!");
        }
    }

    void Update()
    {
        if (lootMaterial == null) return;

        // Определяем целевое значение: 1 если смотрим, 0 если нет
        float targetFocus = isPlayerLooking ? 1f : 0f;

        // Плавно приближаем текущее значение к целевому
        currentFocus = Mathf.MoveTowards(currentFocus, targetFocus, fadeSpeed * Time.deltaTime);

        // Передаем значение в шейдер
        lootMaterial.SetFloat(FocusIntensityID, currentFocus);
    }

    public void SetFocus(bool lookAtMe)
    {
        isPlayerLooking = lookAtMe;
    }
}