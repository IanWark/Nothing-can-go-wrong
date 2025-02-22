using UnityEngine;

public class DeactivateOnClick : MonoBehaviour
{
    [SerializeField]
    private GameObject m_objectToDeactivate;

    public void OnClick()
    {
        m_objectToDeactivate.SetActive(false);
    }
}
