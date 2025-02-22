using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class CrosshairManager : MonoBehaviour
    {
        public Image CrosshairImage;
        public Sprite NullCrosshairSprite;
        public float CrosshairUpdateshrpness = 5f;

        PlayerToolsManager m_WeaponsManager;
        bool m_WasPointingAtEnemy;
        RectTransform m_CrosshairRectTransform;
        ToolController m_CurrentTool;
        CrosshairData m_CurrentCrosshair;

        void Start()
        {
            m_WeaponsManager = FindFirstObjectByType<PlayerToolsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerToolsManager, CrosshairManager>(m_WeaponsManager, this);

            OnWeaponChanged(m_WeaponsManager.GetActiveTool());

            m_WeaponsManager.OnSwitchedToTool += OnWeaponChanged;
        }

        void Update()
        {
            UpdateCrosshairIcon();
            m_WasPointingAtEnemy = m_WeaponsManager.IsPointingAtEnemy;
        }

        void UpdateCrosshairIcon()
        {
            if (m_CurrentTool == null || m_CurrentTool.CrosshairDataDefault.CrosshairSprite == null)
                return;

            if (m_CurrentTool.ShouldUseInteractCrosshair())
            {
                m_CurrentCrosshair = m_CurrentTool.CrosshairDataOnInteract;
            }
            else if (m_WeaponsManager.IsPointingAtEnemy)
            {
                m_CurrentCrosshair = m_CurrentTool.CrosshairDataTargetInSight;
            }
            else if (!m_WeaponsManager.IsPointingAtEnemy)
            {
                m_CurrentCrosshair = m_CurrentTool.CrosshairDataDefault;
            }

            CrosshairImage.sprite = m_CurrentCrosshair.CrosshairSprite;
            m_CrosshairRectTransform.sizeDelta = m_CurrentCrosshair.CrosshairSize * Vector2.one;

            CrosshairImage.color = Color.Lerp(CrosshairImage.color, m_CurrentCrosshair.CrosshairColor,
                Time.deltaTime * CrosshairUpdateshrpness);

            m_CrosshairRectTransform.sizeDelta = Mathf.Lerp(m_CrosshairRectTransform.sizeDelta.x,
                m_CurrentCrosshair.CrosshairSize,
                Time.deltaTime * CrosshairUpdateshrpness) * Vector2.one;
        }

        void OnWeaponChanged(ToolController newWeapon)
        {
            m_CurrentTool = newWeapon;

            if (m_CurrentTool)
            {
                CrosshairImage.enabled = true;
                
                m_CrosshairRectTransform = CrosshairImage.GetComponent<RectTransform>();
                DebugUtility.HandleErrorIfNullGetComponent<RectTransform, CrosshairManager>(m_CrosshairRectTransform,
                    this, CrosshairImage.gameObject);
            }
            else
            {
                if (NullCrosshairSprite)
                {
                    CrosshairImage.sprite = NullCrosshairSprite;
                }
                else
                {
                    CrosshairImage.enabled = false;
                }
            }

            UpdateCrosshairIcon();
        }
    }
}