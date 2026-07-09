using UnityEngine;

public class PlayerLootScanner : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Максимальная дистанция, с которой игрок может 'увидеть' лут")]
    [SerializeField] private float scanDistance = 10f;

    [Tooltip("Слой, на котором находится лут (для оптимизации)")]
    [SerializeField] private LayerMask lootLayer;

    // Ссылка на камеру, из которой пускаем луч
    private Camera playerCamera;

    // Запоминаем лут, на который смотрим прямо сейчас
    private LootLightning currentFocusedLoot = null;

    void Start()
    {
        // Ищем камеру на этом же объекте
        playerCamera = GetComponent<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("Скрипт PlayerLootScanner должен висеть на камере!");
        }
    }

    void Update()
    {
        ScanForLoot();
    }

    private void ScanForLoot()
    {
        // Пускаем луч из центра экрана (камеры) ровно вперед
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Если луч во что-то врезался на заданном расстоянии и на нужном слое...
        if (Physics.Raycast(ray, out hit, scanDistance, lootLayer))
        {
            // Пытаемся получить наш скрипт с объекта, в который попал луч
            LootLightning targetLoot = hit.collider.GetComponentInParent<LootLightning>();

            // Если скрипт найден
            if (targetLoot != null)
            {
                // Если мы только что перевели взгляд на НОВЫЙ предмет лута
                if (currentFocusedLoot != targetLoot)
                {
                    // Выключаем старый (если он был)
                    if (currentFocusedLoot != null)
                    {
                        currentFocusedLoot.SetFocus(false);
                    }

                    // Запоминаем новый и включаем его
                    currentFocusedLoot = targetLoot;
                    currentFocusedLoot.SetFocus(true);
                }

                // Если мы продолжаем смотреть на тот же самый предмет, ничего не делаем
                return;
            }
        }

        // --- Если луч никуда не попал ИЛИ попал в стену (не лут) ---

        // Если у нас в памяти остался включенный лут, выключаем его и забываем
        if (currentFocusedLoot != null)
        {
            currentFocusedLoot.SetFocus(false);
            currentFocusedLoot = null;
        }
    }

    // Рисуем луч в редакторе Unity (окно Scene) для удобства отладки
    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * scanDistance);
        }
    }
}