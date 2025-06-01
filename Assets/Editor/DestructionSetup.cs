#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class DestructionSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Destructible Object From Selection")]
    static void Setup()
    {
        foreach (GameObject selected in Selection.gameObjects)
        {
            Debug.Log($"Setting up destructible object: {selected.name}");

            // Root-Objekt vorbereiten
            Rigidbody rb = selected.GetComponent<Rigidbody>();
            if (rb == null) rb = selected.AddComponent<Rigidbody>();
            rb.mass = 30f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Dummy-Collider hinzufügen und unsichtbar machen
            BoxCollider col = selected.GetComponent<BoxCollider>();
            if (col == null) col = selected.AddComponent<BoxCollider>();
            col.center = new Vector3(0, -9999f, 0);
            col.size = new Vector3(0.1f, 0.1f, 0.1f);

            // DestructionTrigger-Skript hinzufügen
            if (selected.GetComponent<DestructionTrigger>() == null)
                selected.AddComponent<DestructionTrigger>();

            // Alle Kindobjekte durchgehen
            foreach (Transform child in selected.GetComponentsInChildren<Transform>(true))
            {
                if (child == selected.transform) continue;

                // Nur Cube-Objekte bearbeiten
                if (!child.name.StartsWith("Cube")) continue;

                // Alle alten Rigidbody & Joints entfernen
                var oldRb = child.GetComponent<Rigidbody>();
                if (oldRb != null) DestroyImmediate(oldRb);

                var oldFj = child.GetComponent<FixedJoint>();
                if (oldFj != null) DestroyImmediate(oldFj);

                // Collider setzen (falls nicht vorhanden)
                if (child.GetComponent<Collider>() == null)
                    child.gameObject.AddComponent<BoxCollider>();

                // Breakable-Script hinzufügen
                if (child.GetComponent<BreakablePart>() == null)
                    child.gameObject.AddComponent<BreakablePart>();
            }
        }

        Debug.Log("Destructible setup complete for all selected objects.");
    }
}
#endif
