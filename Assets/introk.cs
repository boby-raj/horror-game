using UnityEngine;

public class introk : MonoBehaviour
{
    public GameObject justshit;

    void OnTriggerEnter(Collider other)
    {
      if(other.CompareTag("Player"))
        {
            justshit.SetActive(true);
        }
    }
}
