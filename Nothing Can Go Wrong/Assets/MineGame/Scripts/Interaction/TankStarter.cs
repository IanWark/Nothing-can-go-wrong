using Unity.FPS.Game;
using UnityEngine;

public class StartTankOnInteraction : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ToolEffectType m_toolEffectType;

    [SerializeField]
    private Tank m_tank;

    public void Interact(ToolEffectType toolEffectType)
    {
        if (m_toolEffectType.HasFlag(toolEffectType))
        {
            m_tank.Go();
        }
    }
}
