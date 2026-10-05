#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class LightingFixer : EditorWindow
{
    [MenuItem("Tools/Setup Baked Lighting (Mixed Mode)")]
    public static void FixLighting()
    {
        int lightCount = 0;
        int meshCount = 0;

        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (Light light in allLights)
        {
            Undo.RecordObject(light, "Set Light to Mixed");
            light.lightmapBakeType = LightmapBakeType.Mixed;
            EditorUtility.SetDirty(light);
            lightCount++;
        }

        MeshRenderer[] allRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

        foreach (MeshRenderer renderer in allRenderers)
        {
            Undo.RecordObject(renderer, "Enable Contribute GI");
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
            flags |= StaticEditorFlags.ContributeGI;
            GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, flags);
            renderer.receiveGI = ReceiveGI.Lightmaps;
            EditorUtility.SetDirty(renderer.gameObject);
            meshCount++;
        }

        LightingSettings settings = new LightingSettings();
        settings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
        settings.directSampleCount = 32;
        settings.indirectSampleCount = 128;
        settings.lightmapResolution = 20f;
        settings.lightmapMaxSize = 1024;
        settings.ao = true;
        settings.aoMaxDistance = 1f;
        Lightmapping.lightingSettings = settings;

        Lightmapping.ClearLightingDataAsset();
        Lightmapping.Clear();

        Debug.Log($"[LightingFixer] Done! {lightCount} lights set to Mixed. {meshCount} meshes set to Contribute GI. Old bake cleared.");

        EditorUtility.DisplayDialog(
            "Baked Lighting Setup Complete!",
            $"✅ {lightCount} light(s) set to MIXED mode.\n" +
            $"✅ {meshCount} mesh(es) marked as Contribute GI.\n" +
            $"✅ GPU lightmapper selected (faster bakes).\n" +
            $"✅ Old broken bake data cleared.\n\n" +
            $"NOW: Go to\nWindow → Rendering → Lighting\n→ Click 'Generate Lighting'\n\nYour lights will be baked AND visible!",
            "OK - I'll bake now!"
        );
    }

    [MenuItem("Tools/Clear Bake Data Only")]
    public static void ClearBakeOnly()
    {
        Lightmapping.ClearLightingDataAsset();
        Lightmapping.Clear();
        Debug.Log("[LightingFixer] Bake data cleared.");
        EditorUtility.DisplayDialog("Done", "✅ All baked lightmap data cleared.", "OK");
    }
}
#endif
