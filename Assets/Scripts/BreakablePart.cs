using UnityEngine;

public class BreakablePart : MonoBehaviour
{
    public float breakForce = 2f;

    private bool hasBroken = false;
    private float massOverride = 0.5f;
    private bool applyImpulse = true;
    private GameObject lastHitPlayer = null;
    private float restTime = 0f;
    private const float restThreshold = 0.05f;
    private float ignoreDelayAfterRest = 3f;

    public void SetMass(float mass) => massOverride = mass;
    public void SetApplyImpulse(bool active) => applyImpulse = active;
    void Update()
    {
        if (!hasBroken || lastHitPlayer == null) return;

        var rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        if (rb.linearVelocity.magnitude < restThreshold)

        {
            restTime += Time.deltaTime;

            if (restTime >= ignoreDelayAfterRest)
            {
                DisablePlayerCollision();
            }
        }
        else
        {
            // Wenn sich das Teil wieder bewegt, Timer zurücksetzen
            restTime = 0f;
        }
    }


    public void Detach(float force, GameObject player = null, float delayAfterRest = 3f)
    {
        if (hasBroken) return;
        hasBroken = true;

        ignoreDelayAfterRest = delayAfterRest;
        lastHitPlayer = player;

        transform.SetParent(null);

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

    private void DisablePlayerCollision()
    {
        Collider myCol = GetComponent<Collider>();
        Collider playerCol = lastHitPlayer?.GetComponent<Collider>();

        if (myCol != null && playerCol != null)
        {
            Physics.IgnoreCollision(myCol, playerCol, true);
            lastHitPlayer = null; // aufräumen
        }
    }

}
