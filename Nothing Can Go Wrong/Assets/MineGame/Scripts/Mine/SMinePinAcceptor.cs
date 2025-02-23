using Unity.FPS.Game;
using UnityEngine;

public class SMinePinAcceptor : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ToolEffectType m_acceptedEffectTypes;

    [SerializeField]
    private MineTrigger m_mineTrigger;

    [SerializeField]
    private GameObject m_minePin;

    [SerializeField]
    private Explosive m_explosive;

    public void Interact(ToolEffectType toolEffectType)
    {
        if (m_acceptedEffectTypes.HasFlag(toolEffectType))
        {
            m_mineTrigger.gameObject.SetActive(false);
            m_minePin.SetActive(true);
            m_explosive.Defuse();
        }
    }
}
