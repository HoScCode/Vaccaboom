using UnityEngine;

public class BreakablePart : MonoBehaviour
{
    private float timeWhenStopped = -1f;
    private bool isAtRest = false;
    private bool kinematicSet = false;

    public float breakForce = 2f;
    private bool hasBroken = false;
    private float massOverride = 0.5f;
    private bool applyImpulse = true;
    private GameObject lastHitPlayer = null;
    private const float restThreshold = 0.05f;
    private float ignoreDelayAfterRest = 3f;
    private float kinematicDelayAfterRest = 5f;

    public void SetMass(float mass) => massOverride = mass;
    public void SetApplyImpulse(bool active) => applyImpulse = active;

    void Update()
    {
        if (!hasBroken) return;

        var rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        float velocity = rb.linearVelocity.magnitude;

        if (velocity < restThreshold)
        {
            if (!isAtRest)
            {
                isAtRest = true;
                timeWhenStopped = Time.time;
            }

            float timeSinceStop = Time.time - timeWhenStopped;

            if (lastHitPlayer != null && timeSinceStop >= ignoreDelayAfterRest)
            {
                DisablePlayerCollision();
            }

            if (!kinematicSet && timeSinceStop >= kinematicDelayAfterRest)
            {
                rb.isKinematic = true;
                kinematicSet = true;
            }
        }
        else
        {
            isAtRest = false;
            timeWhenStopped = -1f;
        }
    }

    public void Detach(float force, GameObject player = null, float delayAfterRest = 3f)
    {
        if (hasBroken) return;
        hasBroken = true;

        ignoreDelayAfterRest = delayAfterRest;
        lastHitPlayer = player;

        transform.SetParent(null);

        EnsureCollider();

        var rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = massOverride > 0f ? massOverride : 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (applyImpulse)
        {
            float variation = Random.Range(0.8f, 1.2f);
            Vector3 direction = (Random.insideUnitSphere + transform.up * 0.5f).normalized;
            rb.AddForce(direction * force * variation, ForceMode.Impulse);
        }
    }

    private void EnsureCollider()
    {
        if (GetComponent<Collider>() == null)
        {
            var meshCol = gameObject.AddComponent<MeshCollider>();
            meshCol.convex = true;
        }
    }

    private void DisablePlayerCollision()
    {
        Collider myCol = GetComponent<Collider>();
        Collider playerCol = lastHitPlayer?.GetComponent<Collider>();

        if (myCol != null && playerCol != null)
        {
            Physics.IgnoreCollision(myCol, playerCol, true);
            lastHitPlayer = null;
        }
    }
}
