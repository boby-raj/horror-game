using UnityEngine;

public class introk : MonoBehaviour
{
    public GameObject justshit;
    // Start is called once before the first execution of Update after the MonoBehaviour is createdo
    void OnTriggerEnter(Collider other)
    {
      if(other.CompareTag("Player"))
        {
            justshit.SetActive(true);
        }
    }
}
