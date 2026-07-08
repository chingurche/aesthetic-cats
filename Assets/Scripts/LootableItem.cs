using UnityEngine;

public class LootableItem : InteractableObject
{
    [Header("Linked Data")]
    [SerializeField] private ItemData itemData;

    public override void Interact(Inventory playerInventory)
    {
        if (itemData == null)
        {
            Debug.LogWarning($"На объекте {gameObject.name} не назначен ItemData!");
            return;
        }

        playerInventory.AddItem(itemData);

        Destroy(gameObject);
    }
}