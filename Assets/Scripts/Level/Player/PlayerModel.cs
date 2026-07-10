using System;
using UnityEngine;

[Serializable]
public class PlayerModel
{
    private float depth;
    private float health = 100f;
    private float maxHealth = 100f;
    private float oxygen = 1f;

    public float Depth
    {
        get => depth;
        set
        {
            if (Mathf.Approximately(depth, value))
                return;

            depth = value;
            OnDepthChanged?.Invoke(depth);
        }
    }

    public float Health
    {
        get => health;
        private set
        {
            var clamped = Mathf.Clamp(value, 0f, maxHealth);
            if (Mathf.Approximately(health, clamped))
                return;

            health = clamped;
            OnHealthChanged?.Invoke(health, maxHealth);

            if (health <= 0f)
                OnHealthDepleted?.Invoke();
        }
    }

    public float MaxHealth => maxHealth;

    public float Oxygen
    {
        get => oxygen;
        private set
        {
            var clamped = Mathf.Clamp01(value);
            if (Mathf.Approximately(oxygen, clamped))
                return;

            oxygen = clamped;
            OnOxygenChanged?.Invoke(oxygen);

            if (oxygen <= 0f)
                OnOxygenDepleted?.Invoke();
        }
    }

    public event Action<float> OnDepthChanged;
    public event Action<float, float> OnHealthChanged;
    public event Action<float> OnOxygenChanged;
    public event Action OnHealthDepleted;
    public event Action OnOxygenDepleted;

    public void ResetForRun()
    {
        depth = 0f;
        health = maxHealth;
        oxygen = 1f;

        OnDepthChanged?.Invoke(depth);
        OnHealthChanged?.Invoke(health, maxHealth);
        OnOxygenChanged?.Invoke(oxygen);
    }

    public void DrainOxygen(float amount)
    {
        if (amount <= 0f)
            return;

        Oxygen = oxygen - amount;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f)
            return;

        Health = health - amount;
    }
}
