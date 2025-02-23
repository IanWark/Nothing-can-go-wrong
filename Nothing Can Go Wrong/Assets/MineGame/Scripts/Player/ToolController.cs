using System;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    [System.Serializable]
    public struct CrosshairData
    {
        [Tooltip("The image that will be used for this weapon's crosshair")]
        public Sprite CrosshairSprite;

        [Tooltip("The size of the crosshair image")]
        public int CrosshairSize;

        [Tooltip("The color of the crosshair image")]
        public Color CrosshairColor;
    }

    //[RequireComponent(typeof(AudioSource))]
    public abstract class ToolController : MonoBehaviour
    {
        [Header("Information")]
        [Tooltip("The name that will be displayed in the UI for this tool")]
        [SerializeField]
        public string ToolName;

        [Tooltip("The image that will be displayed in the UI for this weapon")]
        [SerializeField]
        public Sprite ToolIcon;

        [Tooltip("Default data for the crosshair")]
        [SerializeField]
        public CrosshairData CrosshairDataDefault;

        [Tooltip("Data for the crosshair when targeting an enemy")]
        [SerializeField]
        public CrosshairData CrosshairDataTargetInSight;

        [Tooltip("Data for the crosshair when hitting something")]
        [SerializeField]
        public CrosshairData CrosshairDataOnInteract;

        [Tooltip("How long the crosshair changes when hitting something")]
        [SerializeField]
        protected float CrosshairInteractTime;

        [Header("Internal References")]
        [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
        [SerializeField]
        protected GameObject ToolRoot;

        [Header("Aim Parameters")]
        [Tooltip("Ratio of the default FOV that this weapon applies while aiming")]
        [Range(0f, 1f)]
        [SerializeField]
        public float AimZoomRatio = 1f;

        [Header("Ammo Parameters")]
        [Tooltip("Requires ammo")]
        [SerializeField]
        public bool NeedsAmmo = false;
        [Tooltip("Amount of ammo you start with")]
        [SerializeField]
        protected int StartingAmmo = 4;
        [Tooltip("Maximum amount of ammo total")]
        [SerializeField]
        protected int MaxAmmo = 8;

        [Header("Audio & Visual")]
        [Tooltip("Optional tool animator for OnUse and OnReload animations")]
        [SerializeField]
        protected Animator ToolAnimator;

        [Tooltip("Sound played when changing to this tool")]
        [SerializeField]
        private AudioClip ChangeToolSfx;

        [SerializeField]
        protected AudioSource m_ToolAudioSource;

        public GameObject Owner { get; set; }
        public GameObject SourcePrefab { get; set; }
        public bool IsToolActive { get; private set; }

        public Camera ToolCamera { get; set; }

        protected int m_CarriedAmmo;

        public abstract bool ShouldUseInteractCrosshair();

        public int GetCarriedAmmo() => m_CarriedAmmo;
        public void AddCarriableAmmo(int count) => m_CarriedAmmo = Mathf.Max(m_CarriedAmmo + count, MaxAmmo);

        /// <summary>
        /// Handle inputs specific to the tool.
        /// </summary>
        public abstract void HandleInputs(PlayerInputHandler inputHandler);

        protected virtual void Awake()
        {
            m_CarriedAmmo = NeedsAmmo ? StartingAmmo : 0;
        }

        /// <summary>
        /// What happens on switching to or from this tool.
        /// </summary>
        /// <param name="show">Whether the tool is active.</param>
        public void ShowTool(bool show)
        {
            gameObject.SetActive(show);

            if (show && ChangeToolSfx)
            {
                m_ToolAudioSource.PlayOneShot(ChangeToolSfx);
            }

            IsToolActive = show;
        }

    }
}