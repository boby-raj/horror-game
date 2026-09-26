using UnityEngine;

public class box_open_withkey : MonoBehaviour, IInteractable
{
    [Header("Box Settings")]
    public Animator ANI;
    public AudioSource common_Audio;
    
    [Header("UI Texts")]
    [Tooltip("UI Panel that says: 'Press E to Open'")]
    public GameObject open_text;
    
    [Tooltip("UI Panel that says: 'Get the Key'")]
    public GameObject key_text;

    // The key script will turn this to TRUE when you pick it up
    public bool have_key = false; 
    [HideInInspector] public GameObject keyIconToDisable; 
    
    // We use this to make sure it only opens ONCE
    private bool open = false;

    void Start()
    {
        // Hide UI at the start
        if (open_text != null) open_text.SetActive(false);
        if (key_text != null) key_text.SetActive(false);
    }

    public void OnHoverEnter()
    {
        // IF THE BOX IS ALREADY OPEN, DO NOTHING!
        if (open) return; 

        // Show the correct UI text
        if (!have_key)
        {
            if (key_text != null) key_text.SetActive(true); 
        }
        else
        {
            if (open_text != null) open_text.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        // Hide UI when looking away
        if (open_text != null) open_text.SetActive(false);
        if (key_text != null) key_text.SetActive(false);
    }

    public void Interact()
    {
        // 1. If it's already open, ignore the player pressing E
        if (open) return; 

        // 2. Stop the player if they don't have the key
        if (!have_key)
        {
            Debug.Log("The box is locked. Find the key first.");
            return; 
        }

        // 3. --- OPEN THE BOX ONCE ---
        if (common_Audio != null) common_Audio.Play();
        
        if (ANI != null)
        {
            ANI.SetBool("open", true);
            ANI.SetBool("close", false); // Just in case your animator needs this reset
        }
        
        open = true;

        if (keyIconToDisable != null) keyIconToDisable.SetActive(false);

        // 4. Force the UI to turn off instantly since we are done with this box forever
        if (open_text != null) open_text.SetActive(false);
    }
}