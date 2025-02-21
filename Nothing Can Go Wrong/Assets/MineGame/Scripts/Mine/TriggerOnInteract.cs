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
        if (toolEffectType.HasFlag(m_TriggeringToolType))
        {
            m_Explosive.Activate();
        }
    }
}
