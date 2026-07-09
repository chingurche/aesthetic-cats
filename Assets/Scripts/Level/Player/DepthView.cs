using UnityEngine;

public class DepthView : MonoBehaviour
{
    [SerializeField] public Camera rCamera;
    [SerializeField] public float maxDepthEffect;
    [SerializeField] public float minDepthAmbientIntensity;
    [SerializeField] public float maxDepthAmbientIntensity;
    [SerializeField] private Gradient waterGradient;
    [SerializeField] private float minFogDensity = 0.006f;
    [SerializeField] private float maxFogDensity = 0.03f;


    private PlayerModel model;

    public void Initialize(PlayerModel model)
    {
        this.model = model;

        model.OnDepthChanged += SetDepthEffects;

        InitializeEnvironment();
    }

    private void InitializeEnvironment() // потом перенести в какойнибудь более глобальный класс
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.006f;
    }

    private void SetDepthEffects(float depth)
    {
        float t = Mathf.Clamp01(1f - depth / maxDepthEffect);
        t = Mathf.SmoothStep(0f, 1f, t);
        RenderSettings.ambientIntensity = Mathf.Lerp(minDepthAmbientIntensity, maxDepthAmbientIntensity, t);

        RenderSettings.fogDensity = Mathf.Lerp(minFogDensity, maxFogDensity, 1f - t);

        Color c = waterGradient.Evaluate(1f - t);
        RenderSettings.fogColor = c;
        rCamera.backgroundColor = c;
    }

    private void OnDestroy()
    {
        model.OnDepthChanged -= SetDepthEffects;
    }
}
