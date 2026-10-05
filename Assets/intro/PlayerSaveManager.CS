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

        if (PlayerPrefs.GetInt("LoadFromSave", 0) == 1)
        {

            PlayerPrefs.SetInt("LoadFromSave", 0);
            PlayerPrefs.Save();

            float savedX = PlayerPrefs.GetFloat("PlayerX");
            float savedY = PlayerPrefs.GetFloat("PlayerY");
            float savedZ = PlayerPrefs.GetFloat("PlayerZ");

            Debug.Log($"[PlayerSaveManager] Loading saved game! Teleporting player to: ({savedX}, {savedY}, {savedZ})");

            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            transform.position = new Vector3(savedX, savedY, savedZ);

            if (controller != null)
            {
                controller.enabled = true;
            }
        }
        else
        {

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