using UnityEngine;

public class IMGCHANGER : MonoBehaviour
{
    public GameObject tochange;
    public GameObject fromchange;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            tochange.SetActive(true);
            fromchange.SetActive(false);
        }
    }

    void Update()
    {

    }
}
