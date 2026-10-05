using UnityEngine;
using UnityEngine.AI;

public class followscript : MonoBehaviour
{
    public Transform man;
    public NavMeshAgent ag;

    void Start()
    {
        ag=GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        ag.SetDestination(man.position);
        ag.speed=30f;
    }
}
