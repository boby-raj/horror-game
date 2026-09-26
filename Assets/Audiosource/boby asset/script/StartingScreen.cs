using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartingScreen : MonoBehaviour
{
     public GameObject player;
     public GameObject startingScreen;
     public GameObject hud;
     public float waitTime;
    void Start()
    {
        startingScreen.SetActive(true);
        hud.SetActive(false);
        player = GameObject.FindWithTag("Player");
       
        StartCoroutine(Starting());
    }

   IEnumerator Starting()
{
    yield return new WaitForSeconds(waitTime);
    
    startingScreen.SetActive(false);
    hud.SetActive(true);
  
}
    // Update is called once per frame
    void Update()
    {
        
    }
}
