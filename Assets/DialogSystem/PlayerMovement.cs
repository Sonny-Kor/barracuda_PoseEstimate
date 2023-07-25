using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent playerAgent;
    [SerializeField]
    private bool isMoving = false;

    void awake()
    {
        playerAgent = GetComponent<NavMeshAgent>();
    }

    public void MoveToWaypoint(Vector3 destination)
    {
        playerAgent.SetDestination(destination);
        isMoving = true;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public void StopMoving()
    {
        playerAgent.isStopped = true;
        isMoving = false;
    }

    public void ResumeMoving()
    {
        playerAgent.isStopped = false;
        isMoving = true;
    }
}