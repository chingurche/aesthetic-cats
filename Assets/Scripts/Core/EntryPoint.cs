using Core.Economy;
using Core.Gameplay;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Events;
using VContainer;
using UI;

[DefaultExecutionOrder(-100)]
public class EntryPoint : MonoBehaviour
{
    public static EntryPoint Instance { get; private set; }

    public bool IsInitialized { get; private set; }
    public PlayerModel PlayerModel { get; private set; }
    public MainMenuModel EconomyModel { get; private set; }
    public DivingModel DivingModel { get; private set; }

    [SerializeField] private string startSceneAddress;
    [SerializeField] private GameObject uiRootPrefab;

    private IObjectResolver resolver;
    private BaseSceneManager currentSceneManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureUiRoot();
    }

    private void EnsureUiRoot()
    {
        if (GetComponentInChildren<UIManager>(true) != null)
            return;

        if (uiRootPrefab == null)
        {
            Debug.LogWarning("EntryPoint: uiRootPrefab is not assigned. UI will not be available.");
            return;
        }

        Instantiate(uiRootPrefab, transform);
    }

    private async void Start()
    {
        await Initialize();
    }

    private async UniTask Initialize()
    {
        await Addressables.InitializeAsync().ToUniTask();

        var scope = GetComponent<GameLifetimeScope>();
        if (scope == null)
        {
            Debug.LogError("EntryPoint: GameLifetimeScope not found on the same GameObject.");
            return;
        }

        resolver = scope.Container;

        PlayerModel = resolver.Resolve<PlayerModel>();
        EconomyModel = resolver.Resolve<MainMenuModel>();
        DivingModel = resolver.Resolve<DivingModel>();

        var audioSystem = resolver.Resolve<AudioSystem>();
        var saveSystem = resolver.Resolve<SaveSystem>();

        audioSystem.Initialize();
        saveSystem.Initialize(EconomyModel, PlayerModel);

        await LoadScene(startSceneAddress);

        IsInitialized = true;
    }

    private void OnSceneChangedHandler(string sceneName)
    {
        if (currentSceneManager != null)
            currentSceneManager.OnSceneChanged.RemoveListener(OnSceneChangedHandler);

        LoadScene(sceneName).Forget();
    }

    public async UniTask LoadScene(string sceneName)
    {
        var handle = Addressables.LoadSceneAsync(sceneName);
        await handle.ToUniTask();
        await UniTask.WaitUntil(() => handle.Status == AsyncOperationStatus.Succeeded);

        currentSceneManager = FindAnyObjectByType<BaseSceneManager>();
        if (currentSceneManager != null)
        {
            await currentSceneManager.Initialize();
            currentSceneManager.OnSceneChanged.AddListener(OnSceneChangedHandler);
        }
    }
}
