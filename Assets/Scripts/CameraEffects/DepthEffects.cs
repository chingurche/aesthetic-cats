using UnityEngine;

public class DepthEffects : MonoBehaviour
{
    [SerializeField] public DepthCounter depthCounter;
    [SerializeField] public Camera rCamera;
    [SerializeField] public float maxDepthEffect;
    [SerializeField] public float minDepthAmbientIntensity;
    [SerializeField] public float maxDepthAmbientIntensity;
    [SerializeField] public Color minDepthColor;
    [SerializeField] public Color maxDepthColor;


    private void SetDepthEffects(float depth)
    {
        float t = (maxDepthEffect - depth) / maxDepthEffect;
        RenderSettings.ambientIntensity = Mathf.Lerp(minDepthAmbientIntensity, maxDepthAmbientIntensity, t);

        Color c = Color.Lerp(maxDepthColor, minDepthColor, t);
        RenderSettings.fogColor = c;
        rCamera.backgroundColor = c;
    }

    private void OnEnable()
    {
        depthCounter.OnDepthChanged.AddListener(SetDepthEffects);
    }

    private void OnDisable()
    {
        depthCounter.OnDepthChanged.RemoveListener(SetDepthEffects);
    }
}
