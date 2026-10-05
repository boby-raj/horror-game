using UnityEngine;

public class SOUNDHORROR : MonoBehaviour
{
     public AudioSource AUDIOOSOURCE;
     public Animator ANIM;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (AUDIOOSOURCE != null)
            {
                AUDIOOSOURCE.Play();
                ANIM.enabled=true;
            }
        }
    }

}
