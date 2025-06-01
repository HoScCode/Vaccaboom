using UnityEngine;

public class DestructionTrigger : MonoBehaviour
{
    public float breakForce = 2f;

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            var hit = contact.thisCollider;
            var part = hit.GetComponent<BreakablePart>();
            if (part != null)
            {
                part.Detach(breakForce);
            }
        }
    }
}
