using UnityEngine;

public class deadcollide : MonoBehaviour
{
   public GameObject c1;
   public GameObject c2;
   public AudioSource p;

    // Start is called before the first frame update
    void Start()
    {
        c1.SetActive(false);
        c2.SetActive(false);
    }
  
   void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            c1.SetActive(true);
            c2.SetActive(true);
            p.PlayOneShot(p.clip);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
