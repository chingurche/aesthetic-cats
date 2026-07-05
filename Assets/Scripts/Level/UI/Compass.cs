using UnityEngine;

public class Compass : MonoBehaviour
{
    public enum TargetType {Object,Direction};

    [Header("Target")]
    [SerializeField] private TargetType targetType;
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 targetDirection;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform arrow;


    [Header("Spring")]
    [SerializeField] private float spring = 14f;
    [SerializeField] private float damping = 7f;

    private float currentAngle;
    private float angularVelocity;

    private void Update()
    {
        Vector3 direction;
        if (targetType == TargetType.Object)
        {
            direction = target.position - player.position;
            direction.y = 0f;
        }
        else // if (targetType == TargetType.Direction)
        {
            direction = new Vector3(targetDirection.x, 0,targetDirection.y);
        }

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