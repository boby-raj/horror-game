using UnityEngine;

public class gate_open : MonoBehaviour, IInteractable
{
    [Header("Gate Settings")]
    public Animator ANI;
    public AudioSource common_Audio;
    
    [Header("UI Texts")]
    [Tooltip("UI Panel that says: 'Press E to Open'")]
    public GameObject open_text;
    
    [Tooltip("UI Panel that says: 'Press E to Close'")]
    public GameObject close_text;

    // Tracks if the gate is currently open or closed
    private bool open = false;

    void Start()
    {
        // Ensure UI is hidden at the start
        if (open_text != null) open_text.SetActive(false);
        if (close_text != null) close_text.SetActive(false);
        
        // Set default animation states
        if (ANI != null)
        {
            ANI.SetBool("open", false);
            ANI.SetBool("close", true);
        }
    }

    public void OnHoverEnter()
    {
        // Show the correct prompt based on the gate's current state
        if (!open)
        {
            if (open_text != null) open_text.SetActive(true);
        }
        else
        {
            if (close_text != null) close_text.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        // Hide both texts when the player looks away
        if (open_text != null) open_text.SetActive(false);
        if (close_text != null) close_text.SetActive(false);
    }

    public void Interact()
    {
        // Play sound regardless of opening or closing
        if (common_Audio != null)
        {
            common_Audio.Play();
        }

        if (!open)
        {
            // --- OPEN THE GATE ---
            if (ANI != null)
            {
                ANI.SetBool("open", true);
                ANI.SetBool("close", false);
            }
            open = true;

            // Switch the UI text immediately while still looking at it
            if (open_text != null) open_text.SetActive(false);
            if (close_text != null) close_text.SetActive(true);
        }
        else
        {
            // --- CLOSE THE GATE ---
            if (ANI != null)
            {
                ANI.SetBool("open", false);
                ANI.SetBool("close", true);
            }
            open = false;

            // Switch the UI text immediately while still looking at it
            if (close_text != null) close_text.SetActive(false);
            if (open_text != null) open_text.SetActive(true);
        }
    }
}