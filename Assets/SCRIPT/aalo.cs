using System.Collections.Generic;

using UnityEngine;

public class aalo : MonoBehaviour
{       
    public float distance;
      public Camera fpscam;
      public GameObject note;
      public GameObject escreen;
    private bool aaloActivatedEscreen = false;

    void Awake()
    {
        if (fpscam == null) fpscam = GetComponent<Camera>();
        if (fpscam == null) fpscam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (fpscam == null) return;

        Debug.DrawRay(
            fpscam.transform.position,
            fpscam.transform.forward * distance,
            Color.red
        );

        RaycastHit hit;
        if (Physics.Raycast(fpscam.transform.position, fpscam.transform.forward, out hit, distance))
        {
            if (hit.collider != null && hit.collider.gameObject.name == "note")
            {
                if (escreen != null)
                {
                    escreen.SetActive(true);
                    aaloActivatedEscreen = true;
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (note != null) note.SetActive(true);
                }
                return;
            }
        }

        // Only deactivate escreen if aalo was the script that activated it
        if (aaloActivatedEscreen)
        {
            if (escreen != null) escreen.SetActive(false);
            aaloActivatedEscreen = false;
        }
    }
        

    }

