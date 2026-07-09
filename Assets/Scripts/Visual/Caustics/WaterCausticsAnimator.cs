using UnityEngine;

[RequireComponent(typeof(Light))]
public class WaterCausticsAnimator : MonoBehaviour
{
    [SerializeField] private Texture2D[] causticsFrames;
    [SerializeField] private float fps = 20f;

    private Light targetLight;
    private int currentFrame;
    private float timer;
    private float frameTime;

    private void Start()
    {
        targetLight = GetComponent<Light>();

        targetLight.renderMode = LightRenderMode.ForcePixel;

        if (causticsFrames != null && causticsFrames.Length > 0)
        {
            frameTime = 1f / fps;
        }
        else
        {
            enabled = false;
        }
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
    }
}