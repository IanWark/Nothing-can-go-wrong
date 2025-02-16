﻿using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public enum ToolUseType
    {
        Manual,
        Automatic,
        Charge,
    }

    public enum ToolEffectType
    {
        Knife,
    }

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

    [RequireComponent(typeof(AudioSource))]
    public class ToolController : MonoBehaviour
    {
        [Header("Information")]
        [Tooltip("The name that will be displayed in the UI for this tool")]
        [SerializeField]
        public string ToolName;

        [Tooltip("The image that will be displayed in the UI for this weapon")]
        [SerializeField]
        public Sprite ToolIcon;

        [Tooltip("If the tool hits an object, it sends a message saying the object was hit by this type")]
        [SerializeField]
        public ToolEffectType ToolEffectType;

        [Tooltip("Default data for the crosshair")]
        [SerializeField]
        public CrosshairData CrosshairDataDefault;

        [Tooltip("Data for the crosshair when targeting an enemy")]
        [SerializeField]
        public CrosshairData CrosshairDataTargetInSight;

        [Header("Internal References")]
        [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
        [SerializeField]
        private GameObject ToolRoot;

        [Header("Shoot Parameters")]
        [Tooltip("The type of weapon wil affect how it shoots")]
        [SerializeField]
        private ToolUseType ShootType;

        [SerializeField]
        [Tooltip("How far the tool can reach.")]
        private float Reach = 1f;

        [Tooltip("Minimum duration between two shots")]
        [SerializeField]
        private float DelayBetweenShots = 0.5f;

        [Tooltip("Amount of uses used up per use")]
        [SerializeField]
        private int UsesPerUse = 1;

        [Tooltip("Ratio of the default FOV that this weapon applies while aiming")]
        [Range(0f, 1f)]
        [SerializeField]
        public float AimZoomRatio { get; private set; } = 1f;

        [Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
        [SerializeField]
        public Vector3 AimOffset { get; private set; }

        [Header("Ammo Parameters")]
        [Tooltip("Requires ammo")]
        [SerializeField]
        public bool NeedsAmmo = false;
        [Tooltip("Number of uses before needing to reload")]
        [SerializeField]
        private int ClipSize = 1;
        [Tooltip("Maximum amount of ammo total")]
        [SerializeField]
        private int MaxAmmo = 8;

        [Header("Charging parameters (charging weapons only)")]
        [Tooltip("Trigger a shot when maximum charge is reached")]
        [SerializeField]
        private bool AutomaticReleaseOnCharged;

        [Tooltip("Duration to reach maximum charge")]
        [SerializeField]
        private float MaxChargeDuration = 2f;

        [Tooltip("Initial ammo used when starting to charge")]
        [SerializeField]
        private float AmmoUsedOnStartCharge = 1f;

        [Tooltip("Additional ammo used when charge reaches its maximum")]
        [SerializeField]
        private float AmmoUsageRateWhileCharging = 1f;

        [Header("Audio & Visual")]
        [Tooltip("Optional tool animator for OnUse and OnReload animations")]
        [SerializeField]
        private Animator ToolAnimator;

        [Tooltip("sound played when used")]
        [SerializeField]
        private AudioClip UseSfx;

        [Tooltip("Sound played when changing to this tool")]
        [SerializeField]
        private AudioClip ChangeToolSfx;

        [SerializeField]
        private AudioSource m_ToolAudioSource;

        [Header("Continuous Use")]
        [Tooltip("Continuous Shooting Sound")] public bool UseContinuousUseSound = false;
        [SerializeField]
        private AudioClip ContinuousUseStartSfx;
        [SerializeField]
        private AudioClip ContinuousUseLoopSfx;
        [SerializeField]
        private AudioClip ContinuousUseEndSfx;
        [SerializeField]
        private AudioSource m_ContinuousUseAudioSource = null;
        private bool m_WantsToShoot = false;

        [SerializeField]
        private UnityAction OnShoot;
        [SerializeField]
        private event Action OnShootProcessed;

        private int m_CarriedAmmo;
        private float m_CurrentLoadedAmmo;
        private float m_LastTimeShot = Mathf.NegativeInfinity;
        public float LastChargeTriggerTimestamp { get; private set; }

        public GameObject Owner { get; set; }
        public GameObject SourcePrefab { get; set; }
        public bool IsCharging { get; private set; }
        public float CurrentAmmoRatio { get; private set; }
        public bool IsWeaponActive { get; private set; }
        public bool IsCooling { get; private set; }
        public float CurrentCharge { get; private set; }
        public Vector3 MuzzleWorldVelocity { get; private set; }

        public float GetAmmoNeededToShoot() =>
            (ShootType != ToolUseType.Charge ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) /
            (MaxAmmo * UsesPerUse);

        public int GetCarriedAmmo() => m_CarriedAmmo;
        public int GetCurrentLoadedAmmo() => Mathf.FloorToInt(m_CurrentLoadedAmmo);

        public bool IsReloading { get; private set; }

        private Camera toolCamera;
        public void SetToolCamera(Camera camera) { toolCamera = camera; }

        private const string k_AnimUseParameter = "Use";

        void Awake()
        {
            m_CurrentLoadedAmmo = ClipSize;
            m_CarriedAmmo = NeedsAmmo ? ClipSize : 0;

            if (UseContinuousUseSound)
            {
                m_ContinuousUseAudioSource = gameObject.AddComponent<AudioSource>();
                m_ContinuousUseAudioSource.playOnAwake = false;
                m_ContinuousUseAudioSource.clip = ContinuousUseLoopSfx;
                m_ContinuousUseAudioSource.outputAudioMixerGroup =
                    AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);
                m_ContinuousUseAudioSource.loop = true;
            }
        }

        public void AddCarriablePhysicalBullets(int count) => m_CarriedAmmo = Mathf.Max(m_CarriedAmmo + count, MaxAmmo);

        void PlaySFX(AudioClip sfx) => AudioUtility.CreateSFX(sfx, transform.position, AudioUtility.AudioGroups.WeaponShoot, 0.0f);

        void Reload()
        {
            if (m_CarriedAmmo > 0)
            {
                m_CurrentLoadedAmmo = Mathf.Min(m_CarriedAmmo, ClipSize);
            }

            IsReloading = false;
        }

        public void StartReloadAnimation()
        {
            if (m_CurrentLoadedAmmo < m_CarriedAmmo)
            {
                if (ToolAnimator)
                {
                    ToolAnimator.SetTrigger("Reload");
                }
                IsReloading = true;
            }
        }

        void Update()
        {
            UpdateCharge();
            UpdateContinuousUseSound();
        }

        void UpdateContinuousUseSound()
        {
            if (UseContinuousUseSound)
            {
                if (m_WantsToShoot && m_CurrentLoadedAmmo >= 1f)
                {
                    if (!m_ContinuousUseAudioSource.isPlaying)
                    {
                        m_ToolAudioSource.PlayOneShot(UseSfx);
                        m_ToolAudioSource.PlayOneShot(ContinuousUseStartSfx);
                        m_ContinuousUseAudioSource.Play();
                    }
                }
                else if (m_ContinuousUseAudioSource.isPlaying)
                {
                    m_ToolAudioSource.PlayOneShot(ContinuousUseEndSfx);
                    m_ContinuousUseAudioSource.Stop();
                }
            }
        }

        public void ShowTool(bool show)
        {
            ToolRoot.SetActive(show);

            if (show && ChangeToolSfx)
            {
                m_ToolAudioSource.PlayOneShot(ChangeToolSfx);
            }

            IsWeaponActive = show;
        }

        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            m_WantsToShoot = inputDown || inputHeld;
            switch (ShootType)
            {
                case ToolUseType.Manual:
                    if (inputDown)
                    {
                        return TryUse();
                    }

                    return false;

                case ToolUseType.Automatic:
                    if (inputHeld)
                    {
                        return TryUse();
                    }

                    return false;

                case ToolUseType.Charge:
                    if (inputHeld)
                    {
                        TryBeginCharge();
                    }

                    // Check if we released charge or if the weapon shoot autmatically when it's fully charged
                    if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
                    {
                        return TryReleaseCharge();
                    }

                    return false;

                default:
                    return false;
            }
        }

        bool TryUse()
        {
            if (m_CurrentLoadedAmmo >= 1f
                && m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                HandleShoot();
                if (NeedsAmmo)
                {
                    m_CurrentLoadedAmmo -= 1f;
                }

                return true;
            }

            return false;
        }

        void HandleShoot()
        {
            m_LastTimeShot = Time.time;

            // play shoot SFX
            if (UseSfx && !UseContinuousUseSound)
            {
                m_ToolAudioSource.PlayOneShot(UseSfx);
            }

            // Trigger attack animation if there is any
            if (ToolAnimator)
            {
                ToolAnimator.SetTrigger(k_AnimUseParameter);
            }

            // Cast a ray to see if we hit anything
            Vector3 origin = toolCamera.transform.position;
            Vector3 direction = toolCamera.transform.TransformDirection(Vector3.forward);
            LayerMask layerMask = LayerMask.GetMask("Default");
            if (Physics.Raycast(origin, direction, out RaycastHit hit, Reach, layerMask))
            {
                // Try to get the hit object as an IInteractable
                IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    // Let the object know it was hit by a tool of this type.
                    interactable.Interact(ToolEffectType);
                }

                Debug.DrawRay(origin, direction * hit.distance, Color.yellow, DelayBetweenShots);
                Debug.Log($"Did Hit: {hit.collider.gameObject}");
            }
            else
            {
                Debug.DrawRay(origin, direction * Reach, Color.white, DelayBetweenShots);
                Debug.Log("Did not Hit");
            }

            OnShoot?.Invoke();
            OnShootProcessed?.Invoke();
        }

        #region Charging

        void UpdateCharge()
        {
            if (IsCharging)
            {
                if (CurrentCharge < 1f)
                {
                    float chargeLeft = 1f - CurrentCharge;

                    // Calculate how much charge ratio to add this frame
                    float chargeAdded = 0f;
                    if (MaxChargeDuration <= 0f)
                    {
                        chargeAdded = chargeLeft;
                    }
                    else
                    {
                        chargeAdded = (1f / MaxChargeDuration) * Time.deltaTime;
                    }

                    chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

                    // See if we can actually add this charge
                    float ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
                    if (ammoThisChargeWouldRequire <= m_CurrentLoadedAmmo)
                    {
                        // Use ammo based on charge added
                        UseChargedAmmo(ammoThisChargeWouldRequire);

                        // set current charge ratio
                        CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
                    }
                }
            }
        }

        public void UseChargedAmmo(float amount)
        {
            m_CurrentLoadedAmmo = Mathf.Clamp(m_CurrentLoadedAmmo - amount, 0f, MaxAmmo);
            m_CarriedAmmo -= Mathf.RoundToInt(amount);
            m_CarriedAmmo = Mathf.Clamp(m_CarriedAmmo, 0, MaxAmmo);
            m_LastTimeShot = Time.time;
        }

        bool TryBeginCharge()
        {
            if (!IsCharging
                && m_CurrentLoadedAmmo >= AmmoUsedOnStartCharge
                && Mathf.FloorToInt((m_CurrentLoadedAmmo - AmmoUsedOnStartCharge) * UsesPerUse) > 0
                && m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                UseChargedAmmo(AmmoUsedOnStartCharge);

                LastChargeTriggerTimestamp = Time.time;
                IsCharging = true;

                return true;
            }

            return false;
        }

        bool TryReleaseCharge()
        {
            if (IsCharging)
            {
                HandleShoot();

                CurrentCharge = 0f;
                IsCharging = false;

                return true;
            }

            return false;
        }

        #endregion
    }
}