using Unity.FPS.Game;
using UnityEngine;

public class TankObjective : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameOverEvent gameOverEvent = new GameOverEvent();
        gameOverEvent.endGameState = GameFlowManager.EndGameState.Win;
        EventManager.Broadcast(gameOverEvent);
    }
}
