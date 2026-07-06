using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    public GameObject Get(GameObject prefab, Transform parent)
    {
        if (pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            while (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();

                if (obj != null)
                {
                    obj.transform.SetParent(parent, false);
                    obj.SetActive(true);
                    return obj;
                }
            }
        }

        GameObject newObj = Instantiate(prefab, parent);
        newObj.AddComponent<PooledObject>().Prefab = prefab;
        newObj.TryGetComponent(out AtmosphericFishSwarm swarm);
        return newObj;
    }

    public void Release(GameObject obj)
    {
        PooledObject pooled = obj.GetComponent<PooledObject>();

        if (pooled == null)
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform);

        if (!pools.TryGetValue(pooled.Prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools.Add(pooled.Prefab, queue);
        }

        queue.Enqueue(obj);
    }
}