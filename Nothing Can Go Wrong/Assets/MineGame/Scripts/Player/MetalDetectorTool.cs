using System.Collections.Generic;
using UnityEditor;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Experimental.GlobalIllumination;
using System;

namespace Unity.FPS.Game
{
    //[RequireComponent(typeof(AudioSource))]
    public class MetalDetectorTool : ToolController
    {
        [Header("Beep")]
        [Tooltip("Sound effect to play when a mine is detected.")]
        [SerializeField]
        private AudioClip a_BeepSfx;
        [SerializeField]
        private GameObject l_BeepLight;
        [SerializeField]
        private Transform DetectionPointObject;

        private Dictionary<int,DetectedMine> detectedMines;

        private SphereCollider detectionArea;

        protected override void Awake() {
            base.Awake();
            detectionArea = GetComponent<SphereCollider>();
            detectedMines = new Dictionary<int,DetectedMine>();

        }

        private void Update() {
            if (IsToolActive) {
                bool lightOn = false;
                if (detectedMines.Count > 0) {
                    foreach (KeyValuePair<int, DetectedMine> c in detectedMines) {
                        Collider mine = c.Value.mineCollider;
                        Vector3 detectionPoint = DetectionPointObject.position;
                        Vector3 minePoint = mine.ClosestPoint(detectionArea.center);
                        if (minePoint.y <= detectionPoint.y ) {
                            float distance = Math.Abs(Vector3.Distance(minePoint, detectionPoint));
                            if (c.Value.RequiresBeep(distance)) {
                                m_ToolAudioSource.PlayOneShot(a_BeepSfx);
                                l_BeepLight.SetActive(true);
                                lightOn = true;
                            }
                        }
                    }
                }
                if (!lightOn) l_BeepLight.SetActive(false);
            }
        }

        public override void HandleInputs(PlayerInputHandler inputHandler) { }

        private void OnEnable()
        {
            detectedMines.Clear();
        }

        private void OnTriggerEnter(Collider other) {
            if (IsToolActive) {
                if (other.name == "s_mine_charge" || other.name == "SM_Prop_Landmine_01") {
                    int key = other.GetInstanceID();
                    detectedMines.Add(key ,new DetectedMine(other));
                    //Debug.Log($"TriggerEntered - Added {key} {other.name}"); 
                }
            }
        }

        private void OnTriggerExit(Collider other) {
            if (IsToolActive) {
                int key = other.GetInstanceID();
                if (detectedMines.ContainsKey(key)){
                    detectedMines.Remove(key);
                    //Debug.Log($"TriggerExited - Removed: {key} {other.name}");
                }
            }
        }

        public override bool ShouldUseInteractCrosshair() { return true; }
    }

    public class DetectedMine {
        public Collider mineCollider;
        private float lastBeepTime = 0; 
        private static float intervalScale = 0.5f;

        public DetectedMine(Collider mine) {
            mineCollider = mine;
        }

        public bool RequiresBeep(float distance) {
            float currentTime = Time.time;
            float deltaTime = currentTime - lastBeepTime;
            //Debug.Log($"DeltaTime = {deltaTime}");
            //Debug.Log($"Distance = {distance}");
            //Debug.Log($"Interval = {intervalScale / (1/distance)}");
            if ( deltaTime > (intervalScale / (1/distance)) ) {
                lastBeepTime = currentTime;
                return true;
            }
            else return false;
        }
    }
}