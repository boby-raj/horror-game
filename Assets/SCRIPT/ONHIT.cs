using Unity.VisualScripting;
using UnityEngine;

public class ONHIT : MonoBehaviour
{
 public GameObject barrel;

 public AudioSource audiol;
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag=="Player"){
        audiol.Play();}
    }
}
