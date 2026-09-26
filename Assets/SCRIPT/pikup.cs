using Unity.VisualScripting;
using UnityEngine;

public class pikup : MonoBehaviour
{
    public GameObject canvast;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
         canvast.SetActive(true);

        }
        if (Input.GetKey(KeyCode.E)){
  gameObject.SetActive(false);
            
        }
    }
    void OnCollisionExit(Collision collision)
    {
         if (collision.gameObject.tag == "Player")
        {
         canvast.SetActive(false);

        }
        
    }

    // Update is called once per frame
  
}
