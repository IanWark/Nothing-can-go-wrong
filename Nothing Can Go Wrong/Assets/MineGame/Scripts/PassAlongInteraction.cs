using Unity.FPS.Game;
using UnityEngine;

public class PassAlongInteraction : MonoBehaviour, IInteractable
{
    [SerializeField]
    private GameObject objectToPassTo;

    public void Interact(ToolEffectType toolEffectType)
    {
        IInteractable interactable = objectToPassTo.GetComponent<IInteractable>();
        Debug.Assert(interactable != null);
        if (interactable != null)
        {
            interactable.Interact(toolEffectType);
        }
    }
}
