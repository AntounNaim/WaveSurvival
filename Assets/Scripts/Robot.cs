using UnityEngine;
using UnityEngine.AI;
using StarterAssets;

public class Robot : MonoBehaviour
{
    NavMeshAgent agent;
    FirstPersonController player;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        player = FindAnyObjectByType<FirstPersonController>();
    }

    private void OnEnable()
    {
        if (agent != null)
        {
            agent.enabled = false;
            agent.enabled = true;
            agent.Warp(transform.position);
        }

        if (player == null)
            player = FindAnyObjectByType<FirstPersonController>();
    }

    private void Update()
    {
        if (agent.isOnNavMesh)
            agent.SetDestination(player.transform.position);
    }
}
