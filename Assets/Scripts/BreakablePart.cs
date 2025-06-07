using UnityEngine;
using System.Collections.Generic;


public class BreakablePart : MonoBehaviour
{
    private float timeWhenStopped = -1f;
    private bool isAtRest = false;
    private bool kinematicSet = false;
    public bool HasBroken => hasBroken;

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

    //[SerializeField] private float neighborCheckRadius = 0.2f;
    [SerializeField] private float orphanCheckInterval = 1.0f;
    private float orphanCheckTimer = 0f;


    void Update()
    {
        // Sicherheitszeit: Nicht sofort detachen nach Start
        if (!hasBroken && Time.timeSinceLevelLoad < 0.2f)
            return;

         if (!hasBroken)
        {
            orphanCheckTimer += Time.deltaTime;
            if (orphanCheckTimer >= orphanCheckInterval)
            {
                orphanCheckTimer = 0f;
                if (!HasIntactCluster())
                {
                    Detach(breakForce);
                    return;
                }
            }
            return;
        }

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
        transform.root.GetComponent<DestructibleObject>()?.CheckAndAutoDestroy();

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
    private bool HasIntactCluster()
    {
        var thisCollider = GetComponent<Collider>();
        var hits = Physics.OverlapBox(transform.position, thisCollider.bounds.extents * 1.2f, Quaternion.identity);

        var visited = new HashSet<BreakablePart>();
        var queue = new Queue<BreakablePart>();

        queue.Enqueue(this);
        visited.Add(this);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentCollider = current.GetComponent<Collider>();
            if (currentCollider == null) continue;

            Collider[] neighbors = Physics.OverlapBox(current.transform.position, currentCollider.bounds.extents * 1.1f, Quaternion.identity);

            foreach (var col in neighbors)
            {
                if (col == thisCollider || !col.bounds.Intersects(currentCollider.bounds)) continue;

                BreakablePart neighbor = col.GetComponent<BreakablePart>();
                if (neighbor == null || visited.Contains(neighbor)) continue;

                visited.Add(neighbor);

                if (!neighbor.hasBroken)
                {
                    return true; // noch verbunden mit ungebrochener Struktur
                }

                queue.Enqueue(neighbor);
            }
        }

        return false; // kein intakter Nachbar gefunden
    }




}
