using System;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Unity.FPS.Gameplay.PlayerToolsManager;

namespace Unity.FPS.Game
{
    public enum ToolUseType
    {
        Manual,
        Automatic,
    }

    public enum ToolEffectType
    {
        Knife,
    }

    [RequireComponent(typeof(AudioSource))]
    public class ClickToUseTool : ToolController
    {
        [Header("Click To Use Tool")]
        [Tooltip("If the tool hits an object, it sends a message saying the object was hit by this type")]
        [SerializeField]
        private ToolEffectType m_ToolEffectType;

        [Tooltip("Physics layers that will be ignored when raycasting with this tool")]
        [SerializeField]
        private LayerMask LayersToIgnore;

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

        [Header("Ammo Parameters")]
        [Tooltip("Aamount of ammo you start with")]
        [SerializeField]
        private int StartingAmmo = 4;
        [Tooltip("Maximum amount of ammo total")]
        [SerializeField]
        private int MaxAmmo = 8;

        [Header("Audio & Visual")]
        [Tooltip("sound played when used")]
        [SerializeField]
        private AudioClip UseSfx;

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
        private float m_LastTimeShot = Mathf.NegativeInfinity;

        private const string k_AnimUseParameter = "Use";

        public override int GetCarriedAmmo() => m_CarriedAmmo;
        public override void AddCarriableAmmo(int count) => m_CarriedAmmo = Mathf.Max(m_CarriedAmmo + count, MaxAmmo);

        /// <summary>
        /// Handle inputs specific to the tool.
        /// </summary>
        public override void HandleInputs(PlayerInputHandler inputHandler)
        {
            bool inputDown = inputHandler.GetFireInputDown();
            bool inputHeld = inputHandler.GetFireInputHeld();

            m_WantsToShoot = inputDown || inputHeld;
            switch (ShootType)
            {
                case ToolUseType.Manual:
                    if (inputDown)
                    {
                       TryUse();
                    }

                    return;

                case ToolUseType.Automatic:
                    if (inputHeld)
                    {
                        TryUse();
                    }

                    return;
            }
        }

        private void Awake()
        {
            m_CarriedAmmo = NeedsAmmo ? StartingAmmo : 0;

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

        private bool TryUse()
        {
            if (m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                HandleShoot();

                return true;
            }

            return false;
        }

        private void HandleShoot()
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
            Vector3 origin = ToolCamera.transform.position;
            Vector3 direction = ToolCamera.transform.TransformDirection(Vector3.forward);
            LayerMask layerMask = ~LayersToIgnore;
            if (Physics.Raycast(origin, direction, out RaycastHit hit, Reach, layerMask))
            {
                // Try to get the hit object as an IInteractable
                IInteractable[] interactables = hit.collider.gameObject.GetComponents<IInteractable>();
                foreach (IInteractable interactable in interactables)
                {
                    // Let the object know it was hit by a tool of this type.
                    interactable.Interact(m_ToolEffectType);
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

        private void Update()
        {
            UpdateContinuousUseSound();
        }

        private void UpdateContinuousUseSound()
        {
            if (UseContinuousUseSound)
            {
                if (m_WantsToShoot)
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
    }
}