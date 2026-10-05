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

    public bool have_key = false;
    [HideInInspector] public GameObject keyIconToDisable;

    private bool open = false;

    void Start()
    {

        if (open_text != null) open_text.SetActive(false);
        if (key_text != null) key_text.SetActive(false);
    }

    public void OnHoverEnter()
    {

        if (open) return;

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

        if (open_text != null) open_text.SetActive(false);
        if (key_text != null) key_text.SetActive(false);
    }

    public void Interact()
    {

        if (open) return;

        if (!have_key)
        {
            Debug.Log("The box is locked. Find the key first.");
            return;
        }

        if (common_Audio != null) common_Audio.Play();

        if (ANI != null)
        {
            ANI.SetBool("open", true);
            ANI.SetBool("close", false);
        }

        open = true;

        if (keyIconToDisable != null) keyIconToDisable.SetActive(false);

        if (open_text != null) open_text.SetActive(false);
    }
}