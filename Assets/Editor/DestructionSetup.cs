#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DestructionSetupWindow : EditorWindow
{
    private float breakForce = 2f;
    private float partMass = 0.5f;
    private bool applyImpulse = true;

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

        GUILayout.Space(10);

        if (GUILayout.Button("Apply to Selection"))
        {
            ApplySettingsToSelection();
        }

        if (GUILayout.Button("Reset Breakable Parts"))
        {
            ResetSelection();
        }
    }

    void ApplySettingsToSelection()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            var parts = obj.GetComponentsInChildren<BreakablePart>(true);
            foreach (var part in parts)
            {
                part.breakForce = breakForce;
                part.SetMass(partMass);
                part.SetApplyImpulse(applyImpulse);
                EditorUtility.SetDirty(part);
            }
        }

        Debug.Log("Settings applied to selected objects.");
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
}
#endif
