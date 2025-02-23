using Unity.FPS.Game;
using UnityEngine;

public class PickupDefusedMine : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ToolEffectType m_toolEffectType;

    [SerializeField]
    private AudioClip soundFX;

    [SerializeField]
    private Explosive m_explosive;

    public void Interact(ToolEffectType toolEffectType)
    {
        if (m_toolEffectType.HasFlag(toolEffectType) && m_explosive.isDefused)
        {
            if (soundFX != null)
            {
                AudioSource.PlayClipAtPoint(soundFX, transform.position);
            }

            Destroy(m_explosive.gameObject);
        }    
    }
}
