using TMPro;
using Unity.FPS.Game;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public static string GetLastTimeElapsedFormatted()
    {
        return FormatTime(lastTimeElapsed);
    }

    private static float lastTimeElapsed = 0f;

    [SerializeField]
    private TextMeshProUGUI m_timerText;

    private bool isStopped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastTimeElapsed = 0f;

        EventManager.AddListener<GameOverEvent>(OnGameOver);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isStopped)
        {
            lastTimeElapsed += Time.deltaTime;

            m_timerText.text = FormatTime(lastTimeElapsed);
        }
    }

    private static string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnGameOver(GameOverEvent gameOverEvent)
    {
        isStopped = true;
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<GameOverEvent>(OnGameOver);
    }
}
