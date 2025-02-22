using Unity.FPS.Game;
using UnityEngine;

public class DestroyedByKnife : MonoBehaviour, IInteractable
{
    public void Interact(ToolEffectType toolEffectType)
    {
        if (toolEffectType == ToolEffectType.Knife)
        {
            Destroy(gameObject);
        }    
    }
}
