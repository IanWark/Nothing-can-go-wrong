using Unity.FPS.Game;
using UnityEngine;

public class DestroyOnInteract : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ToolEffectType toolEffectType;

    [SerializeField]
    private AudioClip soundFX;

    public void Interact(ToolEffectType toolEffectType)
    {
        if (toolEffectType.HasFlag(toolEffectType))
        {
            if (soundFX != null)
            {
                AudioSource.PlayClipAtPoint(soundFX, transform.position);
            }

            Destroy(gameObject);
        }    
    }
}
