using System.Collections.Generic;
using Infrastructure;
using UnityEngine;
using VContainer;

public class Inventory : MonoBehaviour
{
    private readonly List<ItemData> items = new();
    private DivingGameplayBridge divingBridge;

    [Inject]
    public void Construct(DivingGameplayBridge divingBridge)
    {
        this.divingBridge = divingBridge;
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
        divingBridge?.AddLoot(1);

        Debug.Log($"[Инвентарь] Подобрано: {item.ItemName}. Описание: {item.ItemDescription}");
        Debug.Log($"[Кошелек] Предмет добавлен в забег. Всего предметов: {items.Count}");
    }
}
