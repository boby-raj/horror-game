using UnityEngine;

public class IMGCHANGER : MonoBehaviour
{
    public GameObject tochange;
    public GameObject fromchange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            tochange.SetActive(true);
            fromchange.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
