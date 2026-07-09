using UnityEngine;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Diagnostics;

public class LevelManager : BaseSceneManager
{
    [Inject] private IObjectResolver resolver; 
    [SerializeField] private string mapAddress;
    [SerializeField] private string playerAddress;

    private EndlessTerrain terrain;
    private GameObject playerInstance;

    public override async UniTask Initialize()
    {
        await CreatePlayer();

        await CreateMap();
    }

    private async UniTask CreateMap()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(mapAddress);

        await handle.Task;
        
        GameObject mapPrefab = handle.Result;
        
        GameObject mapInstance = resolver.Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        
        terrain = mapInstance.GetComponent<EndlessTerrain>();
        
        if (terrain != null)
        {
            terrain.Initialize();
        }
        
        ObjectSpawner objectSpawner = mapInstance.GetComponent<ObjectSpawner>();

        if (objectSpawner != null)
        {
            objectSpawner.Initialize();
        }

        WaterCausticsAnimator caustics =mapInstance.GetComponentInChildren<WaterCausticsAnimator>();

        if (caustics != null)
        {
            caustics.Initialize(playerInstance.transform);
        }

        FishManager fishManager = mapInstance.GetComponent<FishManager>();
        if (fishManager != null)
        {
            fishManager.Initialize(playerInstance.transform);
        }
    }

    private async UniTask CreatePlayer()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(playerAddress);
        
        await handle.Task;
        
        GameObject playerPrefab = handle.Result;

        playerInstance = resolver.Instantiate(playerPrefab, new Vector3(0, 10, 0), Quaternion.identity); // НУЖНО ЧТбы СЦЕНА АКТИВИРОВАЛАСЬ ТОЛЬКО ПОСЛЕ ВСЕГо LevelManager.Initiatiate()

        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null) { playerController.Initialize(); }

        PlayerModel playerModel = playerController.Model;

        PlayerView playerView = playerInstance.GetComponentInChildren<PlayerView>(true);
        if (playerView != null) { playerView.Initialize(playerModel); }
        
        DepthView depthView = playerInstance.GetComponent<DepthView>();
        if (depthView != null) { depthView.Initialize(playerModel); }
    }

    public GameObject GetPlayer() => playerInstance;
}