using UnityEngine;

public class SMine : Explosive
{
    public override void Explode(Collider collider)
    {
        Debug.Log($"EXPLODE!!!! collider: {collider.name}");
        // TODO
    }
}
