using UnityEngine;

public class OBSTACLE : MonoBehaviour
{

    public GameObject boxholder;
    void OnTriggerEnter(Collider other)
    {
        boxholder.SetActive(false);
    }
}
