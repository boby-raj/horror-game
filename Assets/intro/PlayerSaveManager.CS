using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    void Start()
    {
        // Check our flag from the Main Menu. Did they click Continue?
        if (PlayerPrefs.GetInt("LoadFromSave", 0) == 1)
        {
            // 1. Retrieve the saved coordinates
            float savedX = PlayerPrefs.GetFloat("PlayerX");
            float savedY = PlayerPrefs.GetFloat("PlayerY");
            float savedZ = PlayerPrefs.GetFloat("PlayerZ");

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
            // If they clicked "New Game", we do nothing. 
            // The player just spawns exactly where you placed them in the Unity editor.
        }
    }
}