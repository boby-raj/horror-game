#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Finds and disables all common UI prompt text objects in the scene.
/// Go to: Tools → Disable All UI Prompt Texts
/// </summary>
public class UIPromptFixer : EditorWindow
{
    // Add any prompt text names your game uses here
    private static readonly string[] promptNames = new string[]
    {
        "open_text",
        "key_text",
        "Press E to Open",
        "Press E",
        "PressE",
        "open text",
        "key text",
        "KeyText",
        "OpenText",
        "Get the Key",
        "Need Key",
        "hoverText",
        "Hover Text",
        "keyAcquiredText",
        "Key Acquired",
        "lev_ui",
        "close_text",
        "escreen",
        "interactPrompt",
        "Interact Prompt"
    };

    [MenuItem("Tools/Disable All UI Prompt Texts")]
    public static void DisableAllPrompts()
    {
        int count = 0;

        // Find every GameObject in scene
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            foreach (string promptName in promptNames)
            {
                if (obj.name.ToLower() == promptName.ToLower() && obj.activeSelf)
                {
                    Undo.RecordObject(obj, "Disable UI Prompt");
                    obj.SetActive(false);
                    EditorUtility.SetDirty(obj);
                    Debug.Log($"[UIPromptFixer] Disabled: {obj.name}");
                    count++;
                    break;
                }
            }
        }

        EditorUtility.DisplayDialog(
            "Done!",
            $"✅ Disabled {count} UI prompt object(s).\n\nThey will now be hidden by default and only appear when the script activates them.",
            "OK"
        );
    }
}
#endif
