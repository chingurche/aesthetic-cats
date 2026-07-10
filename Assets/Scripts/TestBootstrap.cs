using Cysharp.Threading.Tasks;
using UnityEngine;

public class TestBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject playerInstance;


    private async void Start()
    {
        await UniTask.Yield();

        PlayerController controller = FindAnyObjectByType<PlayerController>();

        if (controller != null)
        {
            var playerModel = new PlayerModel();
            playerModel.ResetForRun();
            controller.Initialize(playerModel);

            PlayerView playerView = controller.GetComponent<PlayerView>();
            if (playerView != null)
                playerView.Initialize(playerModel);

            DepthView depthView = controller.GetComponent<DepthView>();
            if (depthView != null)
                depthView.Initialize(playerModel);
        }

        EndlessTerrain terrain = FindAnyObjectByType<EndlessTerrain>();

        if (terrain != null)
        {
            terrain.Initialize();
        }
        else
        {
            Debug.LogError("EndlessTerrain not found.");
        }
        
        ObjectSpawner objectSpawner = FindAnyObjectByType<ObjectSpawner>();
        if (objectSpawner != null)
            objectSpawner.Initialize();

            
        FishManager fishManager = FindAnyObjectByType<FishManager>();
        if (fishManager != null)
        {
            fishManager.Initialize(playerInstance.transform);
        }
    }
}