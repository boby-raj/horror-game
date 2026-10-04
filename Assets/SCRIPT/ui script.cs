
using UnityEngine;
 
public class uiscript : MonoBehaviour
{
    public static string actiontxt;
    public static string normaltext;
    public static bool uiactive;
    [SerializeField] GameObject box;
    [SerializeField] GameObject commsnd;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject crosshair;

    // Update is called once per frame
    void Update()
    {
        if (uiactive == true)
        {
        box.SetActive(true);
        commsnd.SetActive(true);
        crosshair.SetActive(true);
        box.GetComponent<TMPro.TMP_Text>().text=actiontxt;
        }
        else
        {
               box.SetActive(false);
        commsnd.SetActive(false);
        crosshair.SetActive(false);
            
        }
    }
}
