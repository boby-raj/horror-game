using UnityEngine;

public class flahlight : MonoBehaviour
{
 public GameObject lightt;
public AudioSource audioSource;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            lightt.SetActive(!lightt.activeSelf);
            audioSource.Play();
        }

    }
}
