using UnityEngine;

public class jumppp : MonoBehaviour
{
    private Rigidbody rb;
    float velocity=5;

    void Start()
    {
        rb=GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(0,velocity,0);
            velocity+=10;
        }
    }
}
