using UnityEditor;
using UnityEngine;

namespace Unity.FPS.Game
{
    [RequireComponent(typeof(AudioSource))]
    public class ShovelTool : ClickToUseTool
    {
        [Header("Shovel")]
        [Tooltip("Prefab to spawn when digging on dirt.")]
        [SerializeField]
        private GameObject m_ShovelHolePrefab;

        [Tooltip("We'll need to spawn it on top of the ground a bit. How far?")]
        [SerializeField]
        private float m_DistanceAboveGround;

        [SerializeField]
        private AudioClip m_onDigSound;

        protected override void OnHitSomething(RaycastHit hit)
        {
            base.OnHitSomething(hit);

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Dirt"))
            {
                GameObject newHole = PrefabUtility.InstantiatePrefab(m_ShovelHolePrefab) as GameObject;
                newHole.transform.position = hit.point + hit.normal * m_DistanceAboveGround;

                Vector3 target = newHole.transform.position + hit.normal;
                newHole.transform.LookAt(target);

                m_ToolAudioSource.PlayOneShot(m_onDigSound);
            }
        }
    }
}