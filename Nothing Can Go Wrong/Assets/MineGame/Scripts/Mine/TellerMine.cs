using UnityEngine;

public class TellerMine : Explosive
{
    public override void Activate(Collider collider)
    {
        Debug.Log($"EXPLODE MINE!!! Triggered by: {collider.name}");

        Explode();
    }
}
