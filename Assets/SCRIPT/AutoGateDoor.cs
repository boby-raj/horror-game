using UnityEngine;
using System.Collections;

public class AutoGateDoor : MonoBehaviour
{
    [Header("Gate Movement")]
    public Transform gate;              // The actual door/gate mesh to rotate or move
    public float openAngle = 90f;
    public float speed = 2f;

    [Header("Timing")]
    public float autoCloseDelay = 3f;   // Seconds before it closes after opening

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;

    private Coroutine closeRoutine;

[Header("SOUND")]
public AudioSource audioSource;
public AudioClip gateopening;
bool Is_gateopen;
bool Is_gateclose;
    void Start()
    {
        if (gate == null) gate = transform;

        closedRotation = gate.rotation;
        openRotation = Quaternion.Euler(0, openAngle, 0) * closedRotation;
        targetRotation = closedRotation;
    }

    void Update()
    {
        // Smoothly move toward whatever the current target is (open or closed)
        gate.rotation = Quaternion.Slerp(gate.rotation, targetRotation, Time.deltaTime * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OpenGate();
            if(gateopening!=null){
           audioSource.PlayOneShot(gateopening);
        }
        }
    }

    void OpenGate()
    {
        targetRotation = openRotation;

        // If it was already about to close, cancel that and restart the timer
        if (closeRoutine != null)
            StopCoroutine(closeRoutine);

        closeRoutine = StartCoroutine(CloseAfterDelay());
    return;
    }

    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        targetRotation = closedRotation;
        closeRoutine = null;
         audioSource.PlayOneShot(gateopening);
         
    }
}