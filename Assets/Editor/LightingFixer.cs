#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

/// <summary>
/// One-click baked lighting fixer.
/// Go to: Tools → Setup Baked Lighting (Mixed Mode)
/// 
/// What this does:
/// - Sets all lights to MIXED mode (pre-baked shadows + always visible direct light)
/// - Marks all MeshRenderers as Contribute GI so they receive baked light
/// - Sets sensible lightmap resolution settings
/// - Clears old broken bake data
/// 
/// After running this, go to Window → Rendering → Lighting → Generate Lighting
/// </summary>
public class LightingFixer : EditorWindow
{
    [MenuItem("Tools/Setup Baked Lighting (Mixed Mode)")]
    public static void FixLighting()
    {
        int lightCount = 0;
        int meshCount = 0;

        // ── Step 1: Set all lights to Mixed ──────────────────────────────────
        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (Light light in allLights)
        {
            Undo.RecordObject(light, "Set Light to Mixed");
            light.lightmapBakeType = LightmapBakeType.Mixed;
            EditorUtility.SetDirty(light);
            lightCount++;
        }

        // ── Step 2: Mark all MeshRenderers as Contribute GI ──────────────────
        // This is THE most common reason baked lights are invisible —
        // objects must be marked Static/ContributeGI to receive baked lightmaps
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

        // ── Step 3: Set sensible lightmap bake settings ───────────────────────
        LightingSettings settings = new LightingSettings();
        settings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU; // GPU bake = faster
        settings.directSampleCount = 32;
        settings.indirectSampleCount = 128;
        settings.lightmapResolution = 20f;    // Good quality without huge file size
        settings.lightmapMaxSize = 1024;      // 1024 is fine for horror games
        settings.ao = true;                   // Ambient Occlusion for realism
        settings.aoMaxDistance = 1f;
        Lightmapping.lightingSettings = settings;

        // ── Step 4: Clear old broken bake ────────────────────────────────────
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
