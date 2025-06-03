using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class BreakableNetPart : MonoBehaviour
{
    [Header("Verbindungslogik")]
    public float orphanCheckInterval = 1.0f;
    public float minPenetrationDepth = 0.005f;
    public BreakableNetPart rootReference;
    public List<BreakableNetPart> neighbors = new List<BreakableNetPart>();

    [Header("Spieler-Kollision")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float breakImpulseThreshold = 1.0f;

    // intern
    private float orphanTimer = 0f;
    private bool hasBroken = false;

    void Awake()
    {
        BuildNeighborList();
    }

    void Update()
    {
        if (hasBroken) return;

        orphanTimer += Time.deltaTime;
        if (orphanTimer >= orphanCheckInterval)
        {
            orphanTimer = 0f;

            if (!IsConnectedToRoot())
            {
                BreakOff();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasBroken) return;
        if (collision.gameObject.CompareTag(playerTag))
        {
            BreakOff();
        }
    }

    void BuildNeighborList()
    {
        neighbors.Clear();
        var myCollider = GetComponent<Collider>();
        if (myCollider == null) return;

        Collider[] hits = Physics.OverlapBox(
            myCollider.bounds.center,
            myCollider.bounds.extents,
            Quaternion.identity
        );

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            var other = hit.GetComponent<BreakableNetPart>();
            if (other != null && !neighbors.Contains(other))
            {
                if (Physics.ComputePenetration(
                        myCollider, transform.position, transform.rotation,
                        hit, hit.transform.position, hit.transform.rotation,
                        out _, out float dist))
                {
                    if (dist >= minPenetrationDepth)
                    {
                        neighbors.Add(other);

                        // Rückwärts-Verbindung
                        if (!other.neighbors.Contains(this))
                            other.neighbors.Add(this);
                    }
                }
            }
        }
    }

    bool IsConnectedToRoot()
    {
        if (rootReference == null) return true;

        var visited = new HashSet<BreakableNetPart>();
        var queue = new Queue<BreakableNetPart>();
        visited.Add(this);
        queue.Enqueue(this);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == rootReference) return true;

            foreach (var neighbor in current.neighbors)
            {
                if (neighbor == null || visited.Contains(neighbor)) continue;
                if (!neighbor.IsBroken())
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false;
    }

    public void BreakOff()
    {
        if (hasBroken) return;
        hasBroken = true;

        transform.SetParent(null);

        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Vector3 dir = (Random.insideUnitSphere + transform.up * 0.5f).normalized;
            rb.AddForce(dir * 2f, ForceMode.Impulse);
        }
    }

    public void MarkAsBroken() => hasBroken = true;
    public bool IsBroken() => hasBroken;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (var n in neighbors)
        {
            if (n != null)
            {
                Gizmos.DrawLine(transform.position, n.transform.position);
            }
        }
    }
}
