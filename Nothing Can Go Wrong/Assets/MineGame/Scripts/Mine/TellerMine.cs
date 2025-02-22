using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

public class TellerMine : Explosive, IInteractable
{
    [SerializeField]
    private float m_chancePerTripwire;

    [SerializeField]
    private List<GameObject> m_tripwires;

    private void Awake()
    {
        List<GameObject> tripwiresToDestroy = new List<GameObject>();
        foreach (GameObject tripwire in m_tripwires)
        {
            if (Random.Range(0f, 1f) > m_chancePerTripwire)
            {
                Destroy(tripwire);
            }
        }
    }

    public override void Activate()
    {
        Explode();
    }

    public void Interact(ToolEffectType toolEffectType)
    {
        if (toolEffectType == ToolEffectType.Hands)
        {
            bool anyActiveTripwires = false;
            foreach (GameObject tripwire in m_tripwires)
            {
                if (tripwire != null)
                {
                    anyActiveTripwires = true;
                }
            }

            if (anyActiveTripwires)
            {
                Explode();
            }
            else
            {
                // Disarmed
                // TODO
                Destroy(gameObject);
            }
        }
    }
}
