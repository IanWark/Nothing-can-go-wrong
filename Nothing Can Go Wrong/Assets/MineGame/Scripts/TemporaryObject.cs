using UnityEngine;

public class TemporaryObject : MonoBehaviour
{
    [SerializeField]
    private float m_existenceTime = 1f;

    private float m_timeToDie;

    private void Start()
    {
        m_timeToDie = Time.time + m_existenceTime;
    }

    private void Update()
    {
        if (Time.time > m_timeToDie)
        {
            Destroy(gameObject);
        }
    }
}
