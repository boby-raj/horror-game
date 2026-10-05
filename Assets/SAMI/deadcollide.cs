using UnityEngine;

public class deadcollide : MonoBehaviour
{
    public GameObject c1;
    public AudioSource p;

    void Start()
    {
        if (c1 != null)
        {
            c1.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (c1 != null)
            {
                c1.SetActive(true);
            }

            if (p != null)
            {
                p.Play();
            }
        }
    }
}