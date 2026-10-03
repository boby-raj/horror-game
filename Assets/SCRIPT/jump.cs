
using UnityEngine;

public class jump : MonoBehaviour
{
    public CharacterController player;
    public float speed = 10f;
    public float gravity = 9.8f;

    Vector3 velocity;
    float k = 1f;

    void Awake()
    {
        if (player == null)
        {
            player = GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        if (player == null) return;

        // Reset vertical velocity when grounded so gravity does not accumulate indefinitely
        if (player.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
        {
            k = 1.5f;
        }
        else
        {
            k = 1f;
        }

        Vector3 move = transform.right * x + transform.forward * z;
        player.Move(move * speed * k * Time.deltaTime);

        velocity.y -= gravity * Time.deltaTime;
        player.Move(velocity * Time.deltaTime);
    }
}
