using UnityEngine;
using UnityEngine.AI;

public class followscript : MonoBehaviour
{
    public Transform man;
    public NavMeshAgent ag;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ag=GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        ag.SetDestination(man.position);
        ag.speed=30f;
    }
}
