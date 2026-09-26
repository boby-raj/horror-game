using NUnit.Framework;
using UnityEngine;

public class pointlight : MonoBehaviour
{
    public GameObject light1;
    bool Is_lightup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light1.SetActive(false);
        Is_lightup = false;
    }

    // Update is called once per frame
    void Update()
    {
          if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if(Is_lightup==false){
            light1.SetActive(true);
            Is_lightup = true;
            }
            else
            {
                  light1.SetActive(false); 
                  Is_lightup = false;  
            }
        }
    }
}
