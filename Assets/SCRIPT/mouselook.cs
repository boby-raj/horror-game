using UnityEngine;


public class mouselook : MonoBehaviour
{
    public float mousesense=100f;
    float  xrotation =0f;
    public Transform playerbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState=CursorLockMode.Locked;}

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;
        float mouseX=Input.GetAxis("Mouse X")*mousesense * Time.deltaTime;
        float mouseY=Input.GetAxis("Mouse Y")*mousesense * Time.deltaTime;
        xrotation-=mouseY;
        xrotation=Mathf.Clamp(xrotation,-90,90);
        transform.localRotation = Quaternion.Euler(xrotation,0,0);
        playerbody.Rotate(Vector3.up*mouseX);
    }
}
