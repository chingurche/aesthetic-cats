using Core.Gameplay;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

public class LevelManager : BaseSceneManager
{
    [Inject] private IObjectResolver resolver;
    [Inject] private PlayerModel playerModel;
    [Inject] private DivingModel divingModel;
    [Inject] private UIManager uiManager;

    [SerializeField] private string mapAddress;
    [SerializeField] private string playerAddress;
    [SerializeField] private bool autoStartRun = false;

    private EndlessTerrain terrain;
    private GameObject playerInstance;

    public override async UniTask Initialize()
    {
        await CreatePlayer();
        await CreateMap();

        if (autoStartRun && divingModel.State == RunState.Idle)
        {
            playerModel.ResetForRun();
            divingModel.StartRun();
            uiManager.ShowGameplayHUD();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private async UniTask CreateMap()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(mapAddress);
        await handle.Task;

        GameObject mapPrefab = handle.Result;
        GameObject mapInstance = resolver.Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);

        terrain = mapInstance.GetComponent<EndlessTerrain>();
        if (terrain != null)
            terrain.Initialize();

        ObjectSpawner objectSpawner = mapInstance.GetComponent<ObjectSpawner>();
        if (objectSpawner != null)
            objectSpawner.Initialize();

        FishManager fishManager = mapInstance.GetComponent<FishManager>();
        if (fishManager != null)
            fishManager.Initialize(playerInstance.transform);
    }

    private async UniTask CreatePlayer()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(playerAddress);
        await handle.Task;

        GameObject playerPrefab = handle.Result;
        playerInstance = resolver.Instantiate(playerPrefab, new Vector3(0, 10, 0), Quaternion.identity);

        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.Initialize(playerModel);

        PlayerView playerView = playerInstance.GetComponent<PlayerView>();
        if (playerView != null)
            playerView.Initialize(playerModel);

        DepthView depthView = playerInstance.GetComponent<DepthView>();
        if (depthView != null)
            depthView.Initialize(playerModel);
    }

    public GameObject GetPlayer() => playerInstance;
}
