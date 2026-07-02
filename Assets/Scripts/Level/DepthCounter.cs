using UnityEngine;
using UnityEngine.Events;

public class DepthCounter : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float startDepth;
    [SerializeField] private float depthForY;

    [HideInInspector] public UnityEvent<float> OnDepthChanged;

    private float depth;

    void FixedUpdate()
    {
        depth = startDepth - (player.position.y * depthForY);
        OnDepthChanged?.Invoke(depth);
    }
}
