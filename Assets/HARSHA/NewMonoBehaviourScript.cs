using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GhostHUDTrigger : MonoBehaviour
{
    public GhostHUD ghostHUD;
    public string headingText = "WARNING";
    [TextArea(3, 6)]
    public string bodyText = "Your custom spooky text goes here.";
    public bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (ghostHUD == null)
        {
            ghostHUD = FindFirstObjectByType<GhostHUD>(FindObjectsInactive.Include);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnlyOnce && hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            if (ghostHUD != null)
            {
                hasTriggered = true;

                ghostHUD.headingText.text = headingText;
                ghostHUD.bodyText.text = bodyText;

                ghostHUD.TriggerOpen();
            }
        }
    }
}
