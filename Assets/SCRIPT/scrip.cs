using UnityEngine;
using System.Collections;

public class scrip : MonoBehaviour
{
    [Header("GAMEOBJECT")]
    public GameObject sami;
    public Animator anime;
    public AudioClip clip;
    public AudioSource source;

    void Start()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ExecuteWithDelay());
        }
    }


    IEnumerator ExecuteWithDelay()
    {
     

        anime.enabled = true;
        source.PlayOneShot(clip);
        
        yield return new WaitForSeconds(2.0f);
        sami.SetActive(false);
    }

    void Update()
    {
        
    }
}
