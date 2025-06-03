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
        var part = hit.GetComponent<BreakablePart>();
        if (part != null)
        {
            part.Detach(breakForce, collision.gameObject, delayAfterRest);
        }
    }

    destructible?.CheckAndAutoDestroy();
}


}
