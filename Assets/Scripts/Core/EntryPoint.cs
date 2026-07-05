using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

public class EntryPoint : MonoBehaviour
{
    public static EntryPoint Instance { get; private set; }

    public bool IsInitialized { get; private set; }

    private IObjectResolver resolver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await Initialize();
    }

    private async UniTask Initialize()
    {
        await Addressables.InitializeAsync().ToUniTask();

        var scope = FindAnyObjectByType<GameLifetimeScope>();
        if (scope != null) { resolver = scope.Container; }

        AudioSystem audioSystem = resolver.Resolve<AudioSystem>();
        SaveSystem saveSystem = resolver.Resolve<SaveSystem>();

        audioSystem.Initialize();
        saveSystem.Initialize();

        await LoadScene("LevelScene");

        IsInitialized = true;
    }

    public async UniTask LoadScene(string sceneName)
    {
        var handle = Addressables.LoadSceneAsync(sceneName);
        await handle.ToUniTask();

        await UniTask.WaitUntil(() => handle.Status == AsyncOperationStatus.Succeeded);
        /*await Addressables.LoadSceneAsync(sceneName).ToUniTask();
        await UniTask.Delay(100); // ЗДЕСЬ СЦЕНА НЕ УСПЕВАЕТ ПРОГРУЗИТСЯ И НЕ УСПЕВАЕТСЯ НАЙТИ ОБЪЕКТ ЧЗХ?*/

        var sceneManager = FindAnyObjectByType<LevelManager>();
        if (sceneManager != null) { await sceneManager.Initialize(); }
    }
}