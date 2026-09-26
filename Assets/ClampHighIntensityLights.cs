#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public static class ClampHighIntensityLights
{
    private const float Threshold = 50f; // any light with intensity above this will be clamped
    private const float NewIntensity = 10f; // target intensity

    [MenuItem("Tools/Lighting/Clamp High Intensity Lights (set >50 -> 10 and clear cookies)")]
    public static void ClampLights()
    {
        var lights = Resources.FindObjectsOfTypeAll<Light>();
        int changed = 0;
        foreach (var l in lights)
        {
            // skip assets and editor-only lights
            if (EditorUtility.IsPersistent(l) || l.gameObject == null)
                continue;

            if (l.intensity > Threshold)
            {
                Undo.RecordObject(l, "Clamp Light Intensity");
                l.intensity = NewIntensity;
                l.cookie = null;
                EditorUtility.SetDirty(l);
                changed++;
            }
        }

        if (changed > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"ClampHighIntensityLights: adjusted {changed} lights (threshold {Threshold} -> {NewIntensity}).");
        }
        else
        {
            Debug.Log("ClampHighIntensityLights: no lights found above threshold.");
        }
    }
}
#endif
