using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine; // Updated for Unity 6 / Cinemachine 3!

public class OpeningManager : MonoBehaviour
{
    public PlayableDirector timelineDirector;
    public MonoBehaviour playerMovementScript; 
    
    // Add a slot to target the Brain
    public CinemachineBrain mainCameraBrain; 

    void OnEnable()
    {
        timelineDirector.stopped += UnlockControls;
    }

    void OnDisable()
    {
        timelineDirector.stopped -= UnlockControls;
    }

    void Start()
    {
        // Freeze the player the second the game loads
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false; 
        }
    }

    void UnlockControls(PlayableDirector director)
    {
        // 1. Give the player their movement back
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true; 
        }
        
        // 2. Shut down Cinemachine so the Main Camera takes full control!
        if (mainCameraBrain != null)
        {
            mainCameraBrain.enabled = false; 
        }
    }
}