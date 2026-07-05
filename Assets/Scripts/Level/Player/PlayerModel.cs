using UnityEngine;
using System;

[Serializable]
public class PlayerModel
{
    private float depth;

    public float Depth
    {
        get => depth;
        set
        {
            if (Mathf.Approximately(depth, value)) return;
            depth = value;
            OnDepthChanged?.Invoke(depth);
        }
    }

    public event Action<float> OnDepthChanged;
}
