
using UnityEngine;

public class Box_coming : MonoBehaviour
{
    public GameObject objectToActive;
    bool Is_render ;
    private void OnTriggerEnter(Collider other)
    {

        if(other.CompareTag("Player")){
        if (objectToActive != null)
        {
              objectToActive.SetActive(true);
        }
    }
}
}