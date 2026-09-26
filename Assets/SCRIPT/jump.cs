
using System.Xml.Serialization;
using UnityEngine;

public class jump : MonoBehaviour
{ public CharacterController player;

public float speed;
private Rigidbody rb;

public float gravity=9.8f;

Vector3 velocity;
float k=1;

    // Start is called once before the first execution of Update after the MonoBehaviour is crgeteated
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()                                                                    
    {
       float x =Input.GetAxis("Horizontal");
       float z=Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
        {
        k=1.5f;
        }
        else
        {
            k=1;
        }
       Vector3 move =transform.right *x + transform.forward*z;
       player.Move(move*speed*k*Time.deltaTime);
       
       velocity.y-=gravity*Time.deltaTime;
       player.Move(velocity*Time.deltaTime*speed*k);
      
    }
  
}
