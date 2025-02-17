using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Tooltip("Chance that the object will spawn here when the level starts.")]
    [Range(0, 1f)]
    [SerializeField]
    private float SpawnChance = 0.33f;

    private void Awake()
    {
        // Spawn the thing
    }
}
