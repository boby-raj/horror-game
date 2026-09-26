using UnityEngine;

public class lcan : MonoBehaviour
{
    public GameObject thep;
    public GameObject tru;
    public AudioSource source;
    public GameObject escreen;
    public AudioClip sound;

    public bool inarea = false;
    public bool isUsed = false;
   
    void Update()
    {
        if (inarea && !isUsed)
        {
            if (!thep.activeSelf) 
            {
                thep.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                tru.SetActive(true);
                escreen.SetActive(true);
                source.PlayOneShot(sound);
                                
                thep.SetActive(false);
                isUsed = true;
            }
        }
        else if(!isUsed)
        {
            escreen.SetActive(false); 
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inarea = true;
        }  
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inarea = false;
            thep.SetActive(false);
        } 
    }
}