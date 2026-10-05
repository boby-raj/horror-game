using UnityEngine;

public class JUMPSACRE : MonoBehaviour
{
    public AudioClip clip;
  public GameObject wallde;
    public AudioSource sour;
   public GameObject obj;
   [SerializeField]int count=0;
   public GameObject obj2;
   public GameObject obj3;
   bool isin=false;

    void OnTriggerEnter(Collider other)
    {
        obj.SetActive(false);
        obj2.SetActive(true);

        if (Input.GetKeyDown(KeyCode.F))
        {
            sour.PlayOneShot(clip);
           isin=true;
        }
    }
    void Update()
    {
        if(isin==true){
          if (Input.GetKeyDown(KeyCode.F))
        {
         count=count+1;
            if (count == 3)
            {
                obj3.SetActive(false);
                wallde.SetActive(false);
                count=0;
            }}
    }}

}
