using System.Net.NetworkInformation;
using UnityEngine;
 
public class lever_script : MonoBehaviour
{
    public Animator lv_animator;
    public GameObject lev_ui;
    public GameObject MANDATORY;
public GameObject alllights;

    public AudioSource grator;
    public AudioClip start_sound;
    
    private bool inarea = false;
    private bool isPulled = false;
    public lcan can_script;
    public GameObject open_text;
    public GameObject close_text;
    void Start()
    {
        close_text.SetActive(false);
        open_text.SetActive(false);
    }

    void Update()
    {Debug.Log($"inarea:{inarea} isPulled:{isPulled} MANDATORY:{MANDATORY.activeSelf}");
        if (inarea && !isPulled&&MANDATORY.activeSelf==false)
        {
            lev_ui.SetActive(true);
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                lv_animator.enabled = true;
                
                grator.PlayOneShot(start_sound);
                isPulled = true;
                alllights.SetActive(true);
                lev_ui.SetActive(false);
            }
        }
        else
        {
            lev_ui.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inarea = true;
            if(!can_script.isUsed){
             open_text.SetActive(true);
        }else{
              close_text.SetActive(true);
        }
        } 
        
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inarea = false;
            open_text.SetActive(false);
        close_text.SetActive(false);
        } 
        
    }
}