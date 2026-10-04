using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour
{
    public Light myLight;
    public float onDuration = 0.5f;
    public float offDuration = 0.2f;

    void Awake()
    {
        if (myLight == null)
        {
            myLight = GetComponent<Light>();
            if (myLight == null)
            {
                myLight = GetComponentInChildren<Light>();
            }
        }
    }

    void Start()
    {
        if (myLight != null)
        {
            StartCoroutine(Flicker());
        }
        else
        {
            Debug.LogWarning($"[FlickeringLight] No Light component found on {gameObject.name}");
        }
    }

    IEnumerator Flicker()
    {
        while (myLight != null)
        {
            myLight.enabled = true;
            yield return new WaitForSeconds(onDuration);
            if (myLight == null) yield break;
            myLight.enabled = false;
            yield return new WaitForSeconds(offDuration);
        }
    }
}   