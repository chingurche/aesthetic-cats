using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private List<ItemData> items = new List<ItemData>();
    private int totalMoney = 0;

    public void AddItem(ItemData item)
    {
        items.Add(item);
        totalMoney += item.MarketValue;

        Debug.Log($"[Инвентарь] Подобрано: {item.ItemName}. Описание: {item.ItemDescription}");
        Debug.Log($"[Кошелек] +{item.MarketValue} монет. Всего денег: {totalMoney}");
    }
}