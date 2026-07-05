using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Head Bob")]
    [SerializeField] private float frequency = 8f;
    [SerializeField] private float amplitude = 0.05f;
    [SerializeField] private float smoothness = 10f;
    [SerializeField] private float minSpeed = 0.1f;
    [SerializeField] private float horizontalFrequencyMultiplier = 0.5f;
    [SerializeField] private float horizontalAmplitudeMultiplier = 0.5f;

    private Vector3 startLocalPosition;
    private float bobTimer;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        float speed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

        if (speed > minSpeed && playerMovement.IsGrounded)
        {
            bobTimer += Time.deltaTime * frequency * (speed / playerMovement.MaxSpeed);

            Vector3 targetPosition = startLocalPosition;
            targetPosition.y += Mathf.Sin(bobTimer) * amplitude;
            targetPosition.x += Mathf.Cos(bobTimer * horizontalFrequencyMultiplier) * amplitude * horizontalAmplitudeMultiplier;

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                targetPosition,
                smoothness * Time.deltaTime);
        }
        else
        {
            bobTimer = 0f;

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                startLocalPosition,
                smoothness * Time.deltaTime);
        }
    }
}