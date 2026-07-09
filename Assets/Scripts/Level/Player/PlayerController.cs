using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Transform player;
    [SerializeField] private float startDepth;
    [SerializeField] private float depthForY;

    private PlayerModel model;
    public PlayerModel Model => model;

    private bool isInitialized = false;

    public void Initialize()
    {
        model = new PlayerModel();
        
        model.Depth = 0f;
        
        isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!isInitialized) return;

        UpdateDepth();
    }

    private void UpdateDepth()
    {
        model.Depth = startDepth - (player.position.y * depthForY);
    }
}
