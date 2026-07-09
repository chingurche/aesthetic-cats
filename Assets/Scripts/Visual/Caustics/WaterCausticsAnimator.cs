using UnityEngine;

[RequireComponent(typeof(Light))]
public class WaterCausticsAnimator : MonoBehaviour
{
    [SerializeField] private Texture2D[] causticsFrames;
    [SerializeField] private float fps = 20f;
    [SerializeField] private float fadeStartDepth = 120f;
    [SerializeField] private float fadeEndDepth = 250f;
    [SerializeField] private Transform player;

    private float initialIntensity;

    private Light targetLight;
    private int currentFrame;
    private float timer;
    private float frameTime;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;

        targetLight = GetComponent<Light>();
        initialIntensity = targetLight.intensity;

        targetLight.renderMode = LightRenderMode.ForcePixel;

        if (causticsFrames == null || causticsFrames.Length == 0)
        {
            enabled = false;
            return;
        }

        frameTime = 1f / fps;

    }
    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer -= frameTime;
            currentFrame = (currentFrame + 1) % causticsFrames.Length;
            targetLight.cookie = causticsFrames[currentFrame];
        }
        float depth = WorldDepth.YToDepth(player.position.y);

        float t = Mathf.InverseLerp(fadeStartDepth, fadeEndDepth, depth);

        targetLight.intensity = Mathf.Lerp(initialIntensity, 0f, t);
    }
}