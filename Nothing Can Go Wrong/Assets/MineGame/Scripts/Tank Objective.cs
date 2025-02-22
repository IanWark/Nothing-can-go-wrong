using Unity.FPS.Game;
using UnityEngine;

public class TankObjective : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Tank objective");
        EventManager.Broadcast(Events.TankReachedObjective);
    }
}
