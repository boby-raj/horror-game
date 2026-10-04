using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    [Header("Editor Testing")]
    [Tooltip("If checked, the player will ALWAYS spawn at the exact position placed in the scene, ignoring saved game coordinates.")]
    public bool ignoreSaveInEditor = true;

    void Start()
    {
#if UNITY_EDITOR
        if (ignoreSaveInEditor)
        {
            Debug.Log($"[PlayerSaveManager] 'ignoreSaveInEditor' is enabled. Spawning player at scene placed position: {transform.position}");
            return;
        }
#endif

        // Check our flag from the Main Menu. Did they click Continue?
        if (PlayerPrefs.GetInt("LoadFromSave", 0) == 1)
        {
            // Consume the flag so next time you press Play in the Editor, it won't keep overriding your placement
            PlayerPrefs.SetInt("LoadFromSave", 0);
            PlayerPrefs.Save();

            // 1. Retrieve the saved coordinates
            float savedX = PlayerPrefs.GetFloat("PlayerX");
            float savedY = PlayerPrefs.GetFloat("PlayerY");
            float savedZ = PlayerPrefs.GetFloat("PlayerZ");

            Debug.Log($"[PlayerSaveManager] Loading saved game! Teleporting player to: ({savedX}, {savedY}, {savedZ})");

            // 2. CRITICAL UNITY FIX: If your player uses a CharacterController, 
            // you CANNOT teleport them while it is active. It will fight the code.
            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false; // Turn off the physics
            }

            // 3. Teleport the player to the exact spot they quit
            transform.position = new Vector3(savedX, savedY, savedZ);

            // 4. Turn the physics back on
            if (controller != null)
            {
                controller.enabled = true; 
            }
        }
        else
        {
            // If they clicked "New Game" or are testing in editor, we do nothing. 
            // The player just spawns exactly where you placed them in the Unity editor.
            Debug.Log($"[PlayerSaveManager] Spawning player at scene placed position: {transform.position}");
        }
    }

    [ContextMenu("Clear Saved Player Position (Reset to Editor Spawn)")]
    public void ClearSavedPlayerPosition()
    {
        PlayerPrefs.DeleteKey("LoadFromSave");
        PlayerPrefs.DeleteKey("PlayerX");
        PlayerPrefs.DeleteKey("PlayerY");
        PlayerPrefs.DeleteKey("PlayerZ");
        PlayerPrefs.DeleteKey("HasSavedGame");
        PlayerPrefs.Save();
        Debug.Log("[PlayerSaveManager] Saved player position and flags successfully cleared! Player will now spawn where placed in the scene.");
    }
}