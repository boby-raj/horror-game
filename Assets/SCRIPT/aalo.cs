using System.Collections.Generic;

using UnityEngine;

public class aalo : MonoBehaviour
{       
    public float distance;
      public Camera fpscam;
      public GameObject note;
      public GameObject escreen;
    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(
    fpscam.transform.position,
    fpscam.transform.forward * distance,
    Color.red
);


        
        RaycastHit hit;
             if(Physics.Raycast(fpscam.transform.position,fpscam.transform.forward,out hit, distance)){
        
          
                if ((hit.collider.gameObject.name == "note")){
                escreen.SetActive(true);
                if(Input.GetKeyDown(KeyCode.E))
                {
                  note.SetActive(true);
                }
                
           
            }
            else
            {
                escreen.SetActive(false);
            }
        }
        }
        

    }

