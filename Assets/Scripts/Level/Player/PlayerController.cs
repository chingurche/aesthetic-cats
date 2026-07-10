using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Transform player;
    [SerializeField] private float startDepth;
    [SerializeField] private float depthForY;

    private PlayerModel model;
    public PlayerModel Model => model;

    private bool isInitialized;

    public void Initialize(PlayerModel playerModel)
    {
        model = playerModel;
        isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!isInitialized || model == null)
            return;

        UpdateDepth();
    }

    private void UpdateDepth()
    {
        model.Depth = startDepth - (player.position.y * depthForY);
    }
}
