using UnityEngine;
using TMPro;

public class TriggerTagDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tagDisplayMesh;

    private void OnTriggerEnter(Collider other)
    {
        if (tagDisplayMesh != null)
        {
            tagDisplayMesh.text =  "USE "+other.tag;
        }
    }
}
