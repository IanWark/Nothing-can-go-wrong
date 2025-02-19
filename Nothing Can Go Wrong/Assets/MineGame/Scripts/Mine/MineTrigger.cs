using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MineTrigger : MonoBehaviour
{
    [SerializeReference]
    private Explosive m_explosive;

    private void OnTriggerEnter(Collider collider)
    {
        m_explosive.Explode(collider);
    }
}
