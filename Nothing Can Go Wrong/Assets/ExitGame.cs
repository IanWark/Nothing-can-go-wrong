using UnityEngine;

public class ExitGame : MonoBehaviour
{
    
    void QuitGame()
    {
        Application.Quit();

        Debug.Log("Game is exiting");
    }
}
