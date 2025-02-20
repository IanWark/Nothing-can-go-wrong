using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

public class MakeSoundOnInteract : MonoBehaviour, IInteractable
{
    [Serializable]
    private struct AudioClipForEffectType
    {
        [SerializeField]
        public ToolEffectType m_ToolEffectType;
        [SerializeField]
        public AudioClip m_AudioClip;
    }

    [SerializeField]
    private List<AudioClipForEffectType> m_AudioClipsForEffectType;

    [SerializeField]
    private AudioSource m_AudioSource;

    public void Interact(ToolEffectType toolEffectType)
    {
        foreach (AudioClipForEffectType audioClipForEffectType in m_AudioClipsForEffectType)
        {
            if (audioClipForEffectType.m_ToolEffectType == toolEffectType)
            {
                m_AudioSource.PlayOneShot(audioClipForEffectType.m_AudioClip);
                return;
            }
        }
    }
}
