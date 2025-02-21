using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Tooltip("Chance that the object will spawn here when the level starts.")]
    [Range(0, 1f)]
    [SerializeField]
    private float SpawnChance = 0.33f;

    [SerializeField]
    private GameObject ObjectToSpawn;

    private void Awake()
    {
        if (Random.Range(0f, 1f) > SpawnChance)
        {
            Destroy(ObjectToSpawn);
        }
        else
        {
            // Reparent, since we are about to destroy ourselves.
            ObjectToSpawn.transform.parent = transform.parent;
        }

        Destroy(gameObject);
    }
}
