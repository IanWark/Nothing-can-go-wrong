using Unity.FPS.Game;
using UnityEngine;

public class TriggerOnInteract : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ToolEffectType m_TriggeringToolType;

    [SerializeField]
    private Explosive m_Explosive;

    public void Interact(ToolEffectType toolEffectType)
    {
        if (m_TriggeringToolType.HasFlag(toolEffectType))
        {
            m_Explosive.Activate();
        }
    }
}
