using UnityEngine;

public class DepthView : MonoBehaviour
{
    [SerializeField] public Camera rCamera;
    [SerializeField] public float maxDepthEffect;
    [SerializeField] public float minDepthAmbientIntensity;
    [SerializeField] public float maxDepthAmbientIntensity;
    [SerializeField] public Color minDepthColor;
    [SerializeField] public Color maxDepthColor;


    private PlayerModel model;

    public void Initialize(PlayerModel model)
    {
        this.model = model;

        model.OnDepthChanged += SetDepthEffects;
    }

    private void SetDepthEffects(float depth)
    {
        float t = (maxDepthEffect - depth) / maxDepthEffect;
        RenderSettings.ambientIntensity = Mathf.Lerp(minDepthAmbientIntensity, maxDepthAmbientIntensity, t);

        Color c = Color.Lerp(maxDepthColor, minDepthColor, t);
        RenderSettings.fogColor = c;
        rCamera.backgroundColor = c;
    }

    private void OnDestroy()
    {
        model.OnDepthChanged -= SetDepthEffects;
    }
}
