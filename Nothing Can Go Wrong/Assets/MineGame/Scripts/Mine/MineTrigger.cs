using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MineTrigger : MonoBehaviour
{
    [SerializeReference]
    private Explosive m_explosive;

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log($"Trigger activated by: {collider.name}");
        m_explosive.Activate();
    }
}
