using UnityEngine;

public class jumppp : MonoBehaviour
{
    private Rigidbody rb;
    float velocity=5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb=GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(0,velocity,0);
            velocity+=10;
        }
    }
}
