using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

public class Tank : MonoBehaviour
{
    [SerializeField]
    private List<Transform> m_pathPoints;

    [SerializeField]
    private NavMeshAgent m_navMeshAgent;

    [SerializeField]
    private Health m_health;

    [SerializeField]
    private GameObject m_aliveTankModel;

    [SerializeField]
    private GameObject m_destroyedTankModel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        m_health.OnDie += Die;

        Go();
    }

    private void Go()
    {
        m_navMeshAgent.destination = m_pathPoints[0].position;
    }

    private void Die()
    {
        Destroy(m_aliveTankModel);
        m_destroyedTankModel.SetActive(true);
        m_navMeshAgent.isStopped = true;

        // End the game
        EventManager.Broadcast(Events.PlayerDeathEvent);
    }
}
