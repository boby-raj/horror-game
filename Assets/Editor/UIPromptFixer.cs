#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Finds and disables all common UI prompt text objects in the scene,
/// including searching inside text contents (like "PRESS [E] TO OPEN").
/// Go to: Tools → Disable All UI Prompt Texts
/// </summary>
public class UIPromptFixer : EditorWindow
{
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

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (!obj.activeSelf) continue;

            bool shouldDisable = false;

            // 1. Check by GameObject name
            foreach (string promptName in promptNames)
            {
                if (obj.name.Equals(promptName, System.StringComparison.OrdinalIgnoreCase))
                {
                    shouldDisable = true;
                    break;
                }
            }

            // 2. Check TextMeshPro text content (e.g. "PRESS [E] TO OPEN")
            if (!shouldDisable)
            {
                TMP_Text tmp = obj.GetComponent<TMP_Text>();
                if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                {
                    string t = tmp.text.ToUpperInvariant();
                    if (t.Contains("PRESS") && (t.Contains("[E]") || t.Contains(" E ") || t.Contains("OPEN") || t.Contains("KEY") || t.Contains("FUEL")))
                    {
                        shouldDisable = true;
                    }
                }
            }

            // 3. Check Legacy UI Text content
            if (!shouldDisable)
            {
                Text legacyText = obj.GetComponent<Text>();
                if (legacyText != null && !string.IsNullOrEmpty(legacyText.text))
                {
                    string t = legacyText.text.ToUpperInvariant();
                    if (t.Contains("PRESS") && (t.Contains("[E]") || t.Contains(" E ") || t.Contains("OPEN") || t.Contains("KEY") || t.Contains("FUEL")))
                    {
                        shouldDisable = true;
                    }
                }
            }

            if (shouldDisable)
            {
                Undo.RecordObject(obj, "Disable UI Prompt");
                obj.SetActive(false);
                EditorUtility.SetDirty(obj);
                Debug.Log($"[UIPromptFixer] Disabled: {obj.name}");
                count++;
            }
        }

        EditorUtility.DisplayDialog(
            "Done!",
            $"✅ Disabled {count} UI prompt object(s) (including any 'PRESS [E] TO OPEN' texts).\n\nRemember to Save your Scene (Ctrl+S)!",
            "OK"
        );
    }
}
#endif
