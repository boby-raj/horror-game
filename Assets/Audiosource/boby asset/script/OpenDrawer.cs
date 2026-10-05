using UnityEngine;

public class OpenDrawer : MonoBehaviour, IInteractable
{
    [Header("DRAWER SETTINGS")]
    public Animator ANI;
    public AudioSource opensound;
    public AudioSource closesound;
    public AudioSource commmonAudio;

    [Header("UI PANELS / TEXT")]
    public GameObject opentext;
    public GameObject closetext;

    private bool open = false;

    private Renderer meshRenderer;
    private Color originalColor;

    void Start()
    {

        if (opentext != null) opentext.SetActive(false);
        if (closetext != null) closetext.SetActive(false);

        if (ANI != null)
        {
            ANI.SetBool("open", false);
            ANI.SetBool("close", false);
        }

        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    public void OnHoverEnter()
    {

        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.yellow;
        }

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

        if (meshRenderer != null)
        {
            meshRenderer.material.color = originalColor;
        }

        if (opentext != null) opentext.SetActive(false);
        if (closetext != null) closetext.SetActive(false);
    }

    public void Interact()
    {

        if (!open)
        {

            if (opensound != null) opensound.Play();
            if (commmonAudio != null) commmonAudio.Play();

            if (ANI != null)
            {
                ANI.SetBool("open", true);
                ANI.SetBool("close", false);
            }

            open = true;

            if (opentext != null) opentext.SetActive(false);
            if (closetext != null) closetext.SetActive(true);
        }
        else
        {

            if (closesound != null) closesound.Play();
            if (commmonAudio != null) commmonAudio.Play();

            if (ANI != null)
            {
                ANI.SetBool("open", false);
                ANI.SetBool("close", true);
            }

            open = false;

            if (closetext != null) closetext.SetActive(false);
            if (opentext != null) opentext.SetActive(true);
        }
    }
}