using NUnit.Framework;
using UnityEngine;

public class pointlight : MonoBehaviour
{
    public GameObject light1;
    bool Is_lightup;

    void Start()
    {
        light1.SetActive(false);
        Is_lightup = false;
    }

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
