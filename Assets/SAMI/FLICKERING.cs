using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour
{
    public Light myLight;
    public float onDuration = 0.5f;
    public float offDuration = 0.2f;

    void Start()
    {
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            myLight.enabled = true;
            yield return new WaitForSeconds(onDuration);
            myLight.enabled = false;
            yield return new WaitForSeconds(offDuration);
        }
    }
}   