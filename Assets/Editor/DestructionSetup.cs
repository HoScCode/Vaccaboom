#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DestructionSetupWindow : EditorWindow
{
    private float breakForce = 2f;
    private float partMass = 0.5f;
    private bool applyImpulse = true;
    private float kinematicDelayAfterRest = 5f;

    private float baseValue = 100f;
    private float destructionThreshold = 0.2f;

    [MenuItem("Tools/Destruction Setup Window")]
    public static void ShowWindow()
    {
        GetWindow<DestructionSetupWindow>("Destruction Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Destruction Settings", EditorStyles.boldLabel);

        breakForce = EditorGUILayout.FloatField("Break Force", breakForce);
        partMass = EditorGUILayout.FloatField("Part Mass", partMass);
        applyImpulse = EditorGUILayout.Toggle("Apply Impulse", applyImpulse);
        kinematicDelayAfterRest = EditorGUILayout.FloatField("Kinematic Delay", kinematicDelayAfterRest);

        GUILayout.Space(5);
        GUILayout.Label("Value & Scoring", EditorStyles.boldLabel);
        baseValue = EditorGUILayout.FloatField("Base Value ($)", baseValue);
        destructionThreshold = EditorGUILayout.Slider("Auto Detach Threshold", destructionThreshold, 0f, 1f);

        GUILayout.Space(10);

        if (GUILayout.Button("Apply to Selection"))
        {
            ApplySettingsToSelection();
        }

        if (GUILayout.Button("Reset Breakable Parts"))
        {
            ResetSelection();
        }

        if (GUILayout.Button("Remove All Destruction Components"))
        {
            RemoveAllDestructionComponents();
        }
    }

    void ApplySettingsToSelection()
    {
        foreach (GameObject root in Selection.gameObjects)
        {
            // ▶ Root-Setup: Rigidbody, BoxCollider, DestructionTrigger
            Rigidbody rootRb = root.GetComponent<Rigidbody>();
            if (rootRb == null)
            {
                rootRb = root.AddComponent<Rigidbody>();
                rootRb.mass = 30f;
                rootRb.interpolation = RigidbodyInterpolation.Interpolate;
                rootRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }

            BoxCollider rootBox = root.GetComponent<BoxCollider>();
            if (rootBox == null)
            {
                rootBox = root.AddComponent<BoxCollider>();
            }

            rootBox.size = Vector3.one * 0.01f;
            rootBox.center = Vector3.one * 999f;

            DestructionTrigger trigger = root.GetComponent<DestructionTrigger>();
            if (trigger == null)
            {
                trigger = root.AddComponent<DestructionTrigger>();
            }
            trigger.breakForce = breakForce;
            trigger.delayAfterRest = kinematicDelayAfterRest;

            // DestructibleObject hinzufügen und konfigurieren
            DestructibleObject dObj = root.GetComponent<DestructibleObject>();
            if (dObj == null)
            {
                dObj = root.AddComponent<DestructibleObject>();
            }
            dObj.SetBaseValue(baseValue);
            dObj.SetAutoDestroyThreshold(destructionThreshold);

            EditorUtility.SetDirty(rootRb);
            EditorUtility.SetDirty(trigger);
            EditorUtility.SetDirty(dObj);

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (!child.name.ToLower().Contains("cube")) continue;

                var part = child.GetComponent<BreakablePart>();
                if (part == null)
                    part = child.gameObject.AddComponent<BreakablePart>();

                var rb = child.GetComponent<Rigidbody>();
                if (rb != null)
                    DestroyImmediate(rb);

                if (child.GetComponent<Collider>() == null)
                {
                    var meshCol = child.gameObject.AddComponent<MeshCollider>();
                    meshCol.convex = true;
                }

                part.breakForce = breakForce;
                part.SetMass(partMass);
                part.SetApplyImpulse(applyImpulse);

                EditorUtility.SetDirty(part);
            }
        }

        Debug.Log(" Destruction setup applied to selection.");
    }

    void ResetSelection()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            var parts = obj.GetComponentsInChildren<BreakablePart>(true);
            foreach (var part in parts)
            {
                part.breakForce = 0f;
                part.SetMass(0f);
                part.SetApplyImpulse(false);
                EditorUtility.SetDirty(part);
            }
        }

        Debug.Log(" Reset completed.");
    }

    void RemoveAllDestructionComponents()
    {
        foreach (GameObject root in Selection.gameObjects)
        {
            var trigger = root.GetComponent<DestructionTrigger>();
            if (trigger != null) DestroyImmediate(trigger);

            var dObj = root.GetComponent<DestructibleObject>();
            if (dObj != null) DestroyImmediate(dObj);

            var rootRb = root.GetComponent<Rigidbody>();
            if (rootRb != null) DestroyImmediate(rootRb);

            var rootCol = root.GetComponent<Collider>();
            if (rootCol != null && rootCol is MeshCollider) DestroyImmediate(rootCol);

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (!child.name.ToLower().Contains("cube")) continue;

                var part = child.GetComponent<BreakablePart>();
                if (part != null) DestroyImmediate(part);

                var rb = child.GetComponent<Rigidbody>();
                if (rb != null) DestroyImmediate(rb);

                var col = child.GetComponent<Collider>();
                if (col != null && col is MeshCollider)
                    DestroyImmediate(col);
            }
        }

        Debug.Log(" All destruction components removed from selection.");
    }
}
#endif
