using UnityEditor;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class ShovelHole : MonoBehaviour
    {
        [SerializeField]
        private Collider m_collider;

        [SerializeField]
        private float m_amountToMoveMinesUp;

        private void Start()
        {
            Collider[] colliders 
                = Physics.OverlapBox
                (
                    m_collider.transform.position, 
                    m_collider.bounds.extents, 
                    m_collider.transform.rotation, 
                    LayerMask.GetMask("Shovel Hole", "Mine")
                );
            foreach (Collider collider in colliders)
            {
                // Delete any shovel holes we are touching
                ShovelHole existingHole = collider.GetComponent<ShovelHole>();
                if (existingHole && existingHole != this)
                {
                    Destroy(existingHole.transform.parent.gameObject);
                }
                else
                {
                    Transform root = collider.transform.root;

                    // If it is lower than the new hole, move it up to our Y.
                    float newY = Mathf.Max(root.transform.position.y, transform.position.y);
                    root.position = new Vector3(root.position.x, newY, root.position.z);
                }
            }
        }
    }
}