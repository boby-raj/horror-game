using UnityEngine;

public class OBSTACLE : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject boxholder;
    void OnTriggerEnter(Collider other)
    {
        boxholder.SetActive(false);
    }
}
