using UnityEngine;

public class DestructionTrigger : MonoBehaviour
{
    public float breakForce = 2f;
    public float delayAfterRest = 3f;

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        DestructibleObject destructible = GetComponent<DestructibleObject>();

        foreach (ContactPoint contact in collision.contacts)
        {
            var hit = contact.thisCollider;

            // Priorität: Neues System
            var netPart = hit.GetComponent<BreakableNetPart>();
            if (netPart != null && !netPart.IsBroken())
            {
                netPart.BreakOff();
                continue; // keine doppelte Verarbeitung
            }

            // Fallback: Altes System
            var part = hit.GetComponent<BreakablePart>();
            if (part != null && !part.HasBroken)
            {
                part.Detach(breakForce, collision.gameObject, delayAfterRest);
            }
        }

        destructible?.CheckAndAutoDestroy();
    }
}
