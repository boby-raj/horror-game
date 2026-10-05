using System.Collections;
using UnityEngine;

public class deadcollide2 : MonoBehaviour
{
    public GameObject c1;
    public AudioSource p;
    private bool hasTriggered = false;
    private AudioSource fallbackAudio;

    void Awake()
    {
        if (p != null)
        {
            p.playOnAwake = false;
            p.loop = false;
            p.Stop();
        }

        if (c1 != null)
        {
            AudioSource[] childAudios = c1.GetComponentsInChildren<AudioSource>(true);
            foreach (AudioSource a in childAudios)
            {
                if (a != null)
                {
                    a.playOnAwake = false;
                    a.loop = false;
                    a.Stop();
                }
            }
            c1.SetActive(false);
        }
    }

    void Start()
    {
        if (c1 != null && !hasTriggered)
        {
            c1.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            if (c1 != null)
            {
                c1.SetActive(true);
            }

            PlaySoundOnce();

            StartCoroutine(TurnOffAfterDelay(2f));
        }
    }

    private void PlaySoundOnce()
    {
        if (p == null) return;

        p.playOnAwake = false;
        p.loop = false;

        if (p.gameObject.activeInHierarchy && p.enabled)
        {
            p.Stop();
            p.Play();
        }
        else
        {
            if (fallbackAudio == null)
            {
                fallbackAudio = GetComponent<AudioSource>();
                if (fallbackAudio == null)
                {
                    fallbackAudio = gameObject.AddComponent<AudioSource>();
                }
            }

            fallbackAudio.playOnAwake = false;
            fallbackAudio.loop = false;
            fallbackAudio.clip = p.clip;
            fallbackAudio.resource = p.resource;
            fallbackAudio.volume = p.volume;
            fallbackAudio.pitch = p.pitch;
            fallbackAudio.spatialBlend = p.spatialBlend;
            fallbackAudio.Stop();
            fallbackAudio.Play();
        }
    }

    private IEnumerator TurnOffAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (c1 != null)
        {
            c1.SetActive(false);
        }

        if (p != null)
        {
            p.Stop();
        }

        if (fallbackAudio != null)
        {
            fallbackAudio.Stop();
        }
    }
}