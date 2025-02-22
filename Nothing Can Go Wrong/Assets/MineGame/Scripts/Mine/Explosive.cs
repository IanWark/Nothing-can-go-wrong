using Unity.FPS.Game;
using UnityEngine;

public abstract class Explosive : MonoBehaviour
{
    [SerializeField]
    private float AreaOfEffectRadius;
    [Tooltip("How much damage will be dealt by the explosion.")]
    [SerializeField]
    private float BaseDamage;

    /*
    [Tooltip("Damage multiplier over distance for area of effect")]
    public AnimationCurve DamageRatioOverDistance;
    */

    [Header("Audio and visual")]
    [SerializeField]
    private AudioClip ExplosionSound;

    [SerializeField]
    private GameObject ExplosionFX;

    [Header("Debug")]
    [Tooltip("Color of the area of effect radius")]
    [SerializeField]
    private Color DebugAreaOfEffectColor = Color.red * 0.5f;

    private LayerMask layerMask = ~0;
    private const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;

    public abstract void Activate();

    protected void Explode()
    {
        // We are about to destroy the object, so use PlayClipAtPoint instead of our audio source.
        AudioSource.PlayClipAtPoint(ExplosionSound, transform.position);
        // Create an explosion at ourselves
        Instantiate(ExplosionFX, transform.position, transform.rotation);

        InflictDamageInArea(BaseDamage, transform.position, layerMask, k_TriggerInteraction);
        Destroy(gameObject);
    }

    private void InflictDamageInArea(float damage, Vector3 center, LayerMask layers,
    QueryTriggerInteraction interaction)
    {
        // Create a collection of unique health components that would be damaged in the area of effect (in order to avoid damaging a same entity multiple times)
        Collider[] affectedColliders = Physics.OverlapSphere(center, AreaOfEffectRadius, layers, interaction);
        foreach (var coll in affectedColliders)
        {
            Health health = coll.GetComponent<Health>();
            if (health)
            {
                health.TakeDamage(damage);
            }
        }

        /*
        // Apply damages with distance falloff
        foreach (Damageable uniqueDamageable in uniqueDamagedHealths.Values)
        {
            float distance = Vector3.Distance(uniqueDamageable.transform.position, transform.position);
            uniqueDamageable.InflictDamage(
                damage * DamageRatioOverDistance.Evaluate(distance / AreaOfEffectDistance), true, owner);
        }
        */
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = DebugAreaOfEffectColor;
        Gizmos.DrawSphere(transform.position, AreaOfEffectRadius);
    }
}
