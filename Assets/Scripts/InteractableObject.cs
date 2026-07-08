using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public virtual void Interact(Inventory playerInventory)
    {
        Debug.Log("Базовое взаимодействие с " + gameObject.name);
    }
}