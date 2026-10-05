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

    private bool open = false;

    void Start()
    {

        if (open_text != null) open_text.SetActive(false);
        if (close_text != null) close_text.SetActive(false);

        if (ANI != null)
        {
            ANI.SetBool("open", false);
            ANI.SetBool("close", true);
        }
    }

    public void OnHoverEnter()
    {

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

        if (open_text != null) open_text.SetActive(false);
        if (close_text != null) close_text.SetActive(false);
    }

    public void Interact()
    {

        if (common_Audio != null)
        {
            common_Audio.Play();
        }

        if (!open)
        {

            if (ANI != null)
            {
                ANI.SetBool("open", true);
                ANI.SetBool("close", false);
            }
            open = true;

            if (open_text != null) open_text.SetActive(false);
            if (close_text != null) close_text.SetActive(true);
        }
        else
        {

            if (ANI != null)
            {
                ANI.SetBool("open", false);
                ANI.SetBool("close", true);
            }
            open = false;

            if (close_text != null) close_text.SetActive(false);
            if (open_text != null) open_text.SetActive(true);
        }
    }
}