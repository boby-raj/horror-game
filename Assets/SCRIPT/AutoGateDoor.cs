using UnityEngine;
using System.Collections;

public class AutoGateDoor : MonoBehaviour
{
    [Header("Gate Movement")]
    public Transform gate;
    public float openAngle = 90f;
    public float speed = 2f;

    [Header("Timing")]
    public float autoCloseDelay = 3f;

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