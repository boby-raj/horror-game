using UnityEngine;

public class OpenDrawer : MonoBehaviour, IInteractable
{
    [Header("DRAWER SETTINGS")]
    public Animator ANI;
    public AudioSource opensound;
    public AudioSource closesound;
    public AudioSource commmonAudio;

    [Header("UI PANELS / TEXT")]
    public GameObject opentext;  // Put your "Press E to Open" UI here
    public GameObject closetext; // Put your "Press E to Close" UI here

    private bool open = false;

    // Variables for the outline/shine effect
    private Renderer meshRenderer;
    private Color originalColor;

    void Start()
    {
        // 1. Hide the UI text when the game starts
        if (opentext != null) opentext.SetActive(false);
        if (closetext != null) closetext.SetActive(false);
        
        // 2. Reset animations
        if (ANI != null)
        {
            ANI.SetBool("open", false);
            ANI.SetBool("close", false); 
        }

        // 3. Save the original color of the drawer for the hover effect
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    public void OnHoverEnter()
    {
        // THIS HAPPENS WHEN THE RAYCAST HITS THE DRAWER
        
        // 1. Turn the drawer yellow (Outline/Shine effect)
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.yellow;
        }

        // 2. Show the correct text based on if the drawer is open or closed
        if (!open && opentext != null) 
        {
            opentext.SetActive(true);
        }
        else if (open && closetext != null)
        {
            closetext.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        // THIS HAPPENS WHEN THE RAYCAST LEAVES THE DRAWER
        
        // 1. Revert the color back to normal
        if (meshRenderer != null)
        {
            meshRenderer.material.color = originalColor;
        }

        // 2. Hide all text
        if (opentext != null) opentext.SetActive(false);
        if (closetext != null) closetext.SetActive(false);
    }

    public void Interact()
    {
        // THIS HAPPENS WHEN YOU PRESS 'E'
        if (!open)
        {
            // --- OPEN LOGIC ---
            if (opensound != null) opensound.Play();
            if (commmonAudio != null) commmonAudio.Play();
            
            if (ANI != null)
            {
                ANI.SetBool("open", true);
                ANI.SetBool("close", false);
            }
            
            open = true;
            
            // Swap the UI text immediately while you are still looking at it
            if (opentext != null) opentext.SetActive(false);
            if (closetext != null) closetext.SetActive(true);
        }
        else
        {
            // --- CLOSE LOGIC ---
            if (closesound != null) closesound.Play();
            if (commmonAudio != null) commmonAudio.Play();

            if (ANI != null)
            {
                ANI.SetBool("open", false);
                ANI.SetBool("close", true);
            }
            
            open = false;
            
            // Swap the UI text immediately while you are still looking at it
            if (closetext != null) closetext.SetActive(false);
            if (opentext != null) opentext.SetActive(true);
        }
    }
}