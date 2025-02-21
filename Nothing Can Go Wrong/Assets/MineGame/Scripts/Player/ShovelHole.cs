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
                    m_collider.bounds.extents / 2, 
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
                    // I had issues getting the root of the object without going too far up the heirarchy.
                    // So tag the root of things that should move with "Diggable"
                    Transform diggable = collider.transform;
                    while(diggable != null)
                    {
                        if (diggable.tag == "Diggable")
                        {
                            break;
                        }

                        diggable = diggable.parent;
                    }

                    if (diggable != null)
                    {
                        Transform root = diggable.gameObject.transform;
                        // If it is lower than the new hole, move it up to the hole's Y.
                        float newY = Mathf.Max(root.position.y, transform.position.y);
                        root.position = new Vector3(root.position.x, newY, root.position.z);
                    }
                }
            }
        }
    }
}