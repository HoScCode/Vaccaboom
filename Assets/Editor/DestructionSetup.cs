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

    private enum Mode { BreakableParts, BreakableNet }
    private Mode setupMode = Mode.BreakableParts;

    [MenuItem("Tools/Destruction Setup Window")]
    public static void ShowWindow()
    {
        GetWindow<DestructionSetupWindow>("Destruction Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Destruction Settings", EditorStyles.boldLabel);

        setupMode = (Mode)EditorGUILayout.EnumPopup("Setup Mode", setupMode);

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
            ApplySettingsToSelection();

        if (GUILayout.Button("Reset Breakable Parts"))
            ResetSelection();

        if (GUILayout.Button("Remove All Destruction Components"))
            RemoveAllDestructionComponents();
    }

    void ApplySettingsToSelection()
    {
        foreach (GameObject root in Selection.gameObjects)
        {
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

            var trigger = root.GetComponent<DestructionTrigger>();
            if (trigger == null)
                trigger = root.AddComponent<DestructionTrigger>();

            trigger.breakForce = breakForce;
            trigger.delayAfterRest = kinematicDelayAfterRest;

            var dObj = root.GetComponent<DestructibleObject>();
            if (dObj == null)
                dObj = root.AddComponent<DestructibleObject>();

            dObj.SetBaseValue(baseValue);
            dObj.SetAutoDestroyThreshold(destructionThreshold);

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (!child.name.ToLower().Contains("cube")) continue;

                if (setupMode == Mode.BreakableParts)
                {
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
                }
                else // BreakableNetPart
                {
                    var net = child.GetComponent<BreakableNetPart>();
                    if (net == null)
                        net = child.gameObject.AddComponent<BreakableNetPart>();

                    if (child.GetComponent<Collider>() == null)
                        child.gameObject.AddComponent<MeshCollider>();
                }

                EditorUtility.SetDirty(child.gameObject);
            }

            EditorUtility.SetDirty(root);
        }

        Debug.Log("Destruction setup applied to selection.");
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

        Debug.Log("Reset completed.");
    }

    void RemoveAllDestructionComponents()
    {
        foreach (GameObject root in Selection.gameObjects)
        {
            // Root-Komponenten
            DestroyImmediate(root.GetComponent<DestructionTrigger>());
            DestroyImmediate(root.GetComponent<DestructibleObject>());
            DestroyImmediate(root.GetComponent<Rigidbody>());

            var rootCol = root.GetComponent<Collider>();
            if (rootCol != null && rootCol is BoxCollider) DestroyImmediate(rootCol);

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (!child.name.ToLower().Contains("cube")) continue;

                DestroyImmediate(child.GetComponent<BreakablePart>());
                DestroyImmediate(child.GetComponent<BreakableNetPart>());
                DestroyImmediate(child.GetComponent<Rigidbody>());

                var col = child.GetComponent<Collider>();
                if (col != null && col is MeshCollider)
                    DestroyImmediate(col);
            }
        }

        Debug.Log("All destruction components removed from selection.");
    }
}
#endif
