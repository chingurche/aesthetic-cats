using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Base Info")]
    [SerializeField] private string itemName = "Название предмета";
    [TextArea(3, 5)]
    [SerializeField] private string itemDescription = "Описание предмета";

    [Header("Economy")]
    [SerializeField] private int marketValue = 10;

    public string ItemName => itemName;
    public string ItemDescription => itemDescription;
    public int MarketValue => marketValue;
}