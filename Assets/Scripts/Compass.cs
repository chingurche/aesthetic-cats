using UnityEngine;

public class Compass : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform depthTarget;
    [SerializeField] private RectTransform arrow;


    [Header("Spring")]
    [SerializeField] private float spring = 14f;
    [SerializeField] private float damping = 7f;

    private float currentAngle;
    private float angularVelocity;

    private void Update()
    {
        Vector3 direction = depthTarget.position - player.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        float targetAngle = -Vector3.SignedAngle(
            player.forward,
            direction.normalized,
            Vector3.up);

        float delta = Mathf.DeltaAngle(currentAngle, targetAngle);

        angularVelocity += delta * spring * Time.deltaTime;
        angularVelocity *= Mathf.Exp(-damping * Time.deltaTime);

        currentAngle += angularVelocity * Time.deltaTime;

        arrow.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }
}