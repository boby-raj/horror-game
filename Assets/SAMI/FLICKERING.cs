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

    void OnEnable()
    {
        StopAllCoroutines();
        if (myLight != null)
        {
            StartCoroutine(Flicker());
        }
        else
        {
            Debug.LogWarning($"[FlickeringLight] No Light component found on {gameObject.name}");
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (myLight != null)
        {
            myLight.enabled = false;
        }
    }

    IEnumerator Flicker()
    {
        while (myLight != null && enabled && gameObject.activeInHierarchy)
        {
            myLight.enabled = true;
            yield return new WaitForSeconds(onDuration);
            if (myLight == null || !enabled || !gameObject.activeInHierarchy) yield break;
            myLight.enabled = false;
            yield return new WaitForSeconds(offDuration);
        }
    }
}