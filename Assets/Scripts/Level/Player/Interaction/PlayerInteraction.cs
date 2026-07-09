using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Color highlightColor = Color.yellow;

    private Camera mainCamera;
    private Renderer currentRenderer;
    private Color originalColor;
    private InteractableObject currentInteractable;
    private Inventory playerInventory;

    void Start()
    {
        mainCamera = Camera.main;
        playerInventory = GetComponent<Inventory>();
    }

    void Update()
    {
        CheckForInteractable();

        bool isEPressed = false;
        var keyboard = UnityEngine.InputSystem.Keyboard.current;

        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            isEPressed = true;
        }

        if (currentInteractable != null && isEPressed)
        {
            currentInteractable.Interact(playerInventory);
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                if (interactable != currentInteractable)
                {
                    ClearHighlight();
                    currentInteractable = interactable;
                    currentRenderer = hit.collider.GetComponent<Renderer>();

                    if (currentRenderer != null)
                    {
                        originalColor = currentRenderer.material.color;
                        currentRenderer.material.color = highlightColor;
                    }
                }
                return;
            }
        }
        ClearHighlight();
    }

    private void ClearHighlight()
    {
        if (currentInteractable != null)
        {
            if (currentRenderer != null)
            {
                currentRenderer.material.color = originalColor;
            }
            currentInteractable = null;
            currentRenderer = null;
        }
    }
}