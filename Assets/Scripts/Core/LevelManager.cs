using UnityEngine;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Diagnostics;

public class LevelManager : MonoBehaviour, ISceneManager
{
    [Inject] private IObjectResolver resolver; 
    [SerializeField] private string mapAddress;
    [SerializeField] private string playerAddress;

    private EndlessTerrain terrain;
    private GameObject playerInstance;

    public async UniTask Initialize()
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

        PlayerView playerView = playerInstance.GetComponent<PlayerView>();
        if (playerView != null) { playerView.Initialize(playerModel); }
        
        DepthView depthView = playerInstance.GetComponent<DepthView>();
        if (depthView != null) { depthView.Initialize(playerModel); }
    }

    public GameObject GetPlayer() => playerInstance;
}