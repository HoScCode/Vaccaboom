using UnityEngine;

public class BreakablePart : MonoBehaviour
{
    private bool hasBroken = false;

    public void Detach(float force)
    {
        if (hasBroken) return;
        hasBroken = true;

        transform.SetParent(null);

        var rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.AddForce(Random.onUnitSphere * force, ForceMode.Impulse);
    }
}
