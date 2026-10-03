using UnityEngine;

public class lever_script : MonoBehaviour
{
    [Header("Generator & Lever Controls")]
    public Animator lv_animator;
    public GameObject lev_ui;
    public GameObject MANDATORY;
    public GameObject alllights;

    [Header("Audio")]
    public AudioSource grator;
    public AudioClip start_sound;

    [Header("Fuel Check")]
    public lcan can_script;
    public GameObject open_text;
    public GameObject close_text;

    private bool inarea = false;
    private bool isPulled = false;

    void Start()
    {
        if (close_text != null) close_text.SetActive(false);
        if (open_text != null) open_text.SetActive(false);
        if (lev_ui != null) lev_ui.SetActive(false);
    }

    void Update()
    {
        if (isPulled) return;

        bool hasFuel = (can_script != null && can_script.isUsed) || 
                       (MANDATORY != null && !MANDATORY.activeSelf);

        if (inarea && !isPulled && hasFuel)
        {
            if (lev_ui != null && !lev_ui.activeSelf) lev_ui.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (lv_animator != null) lv_animator.enabled = true;
                if (grator != null && start_sound != null) grator.PlayOneShot(start_sound);
                isPulled = true;
                if (alllights != null) alllights.SetActive(true);
                if (lev_ui != null) lev_ui.SetActive(false);
                if (open_text != null) open_text.SetActive(false);
                if (close_text != null) close_text.SetActive(false);

                if (GeneratorSystem.Instance != null)
                {
                    GeneratorSystem.Instance.AddFuel(60f);
                }
            }
        }
        else
        {
            if (lev_ui != null && lev_ui.activeSelf) lev_ui.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inarea = true;

            bool hasFuel = (can_script != null && can_script.isUsed) || 
                           (MANDATORY != null && !MANDATORY.activeSelf);

            if (!hasFuel)
            {
                if (open_text != null) open_text.SetActive(true);
            }
            else
            {
                if (close_text != null) close_text.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inarea = false;
            if (open_text != null) open_text.SetActive(false);
            if (close_text != null) close_text.SetActive(false);
            if (lev_ui != null) lev_ui.SetActive(false);
        }
    }
}