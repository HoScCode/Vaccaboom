using UnityEngine;
using System.Collections.Generic;

public class DestructibleObject : MonoBehaviour
{
    [SerializeField] private float baseValue = 100f;
    [SerializeField] private float autoDestroyThreshold = 0.2f;

    public float BaseValue => baseValue;
    public float AutoDestroyThreshold => autoDestroyThreshold;

    private List<BreakablePart> parts;
    private bool isDestroyed = false;
    public void SetBaseValue(float val) => baseValue = val;
    public void SetAutoDestroyThreshold(float val) => autoDestroyThreshold = val;
    
    void Awake()
    {
        parts = new List<BreakablePart>(GetComponentsInChildren<BreakablePart>());
    }

    public float GetDestructionRatio()
    {
        int total = parts.Count;
        int broken = 0;

        foreach (var part in parts)
        {
            if (part.HasBroken) broken++;
        }

        return total > 0 ? (float)broken / total : 0f;
    }

    public float GetCurrentValue()
    {
        return baseValue * GetDestructionRatio();
    }

    public void CheckAndAutoDestroy()
    {
        float destruction = GetDestructionRatio();
        float value = GetCurrentValue();

        Debug.Log($"Zerstörung: {(destruction * 100f):F1}% | $ Verdient: {value:F1} $");

        if (!isDestroyed && (1f - destruction) <= autoDestroyThreshold)
        {
            Debug.Log($" Fast alles zerstört – Rest fällt ab.");

            foreach (var part in parts)
            {
                if (!part.HasBroken)
                {
                    part.Detach(0f); // kein Impuls
                }
            }

            isDestroyed = true;
        }
    }
}
