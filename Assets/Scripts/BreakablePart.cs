using UnityEngine;

public class BreakablePart : MonoBehaviour
{
    public float breakForce = 2f;

    private bool hasBroken = false;
    private float massOverride = 0.5f;
    private bool applyImpulse = true;

    public void SetMass(float mass) => massOverride = mass;
    public void SetApplyImpulse(bool active) => applyImpulse = active;

    public void Detach(float force)
    {
        if (hasBroken) return;
        hasBroken = true;

        transform.SetParent(null);

        var rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = massOverride > 0f ? massOverride : 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (applyImpulse)
        {
            rb.AddForce(Random.onUnitSphere * force, ForceMode.Impulse);
        }
    }
}
